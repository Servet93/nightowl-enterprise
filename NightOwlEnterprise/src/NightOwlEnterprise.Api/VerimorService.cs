using System.Text.Json;
using Hangfire;
using Hangfire.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using NightOwlEnterprise.Api.Entities;

namespace NightOwlEnterprise.Api;

public class VerimorService
{
    private readonly string apiBulutSantral =
        "https://api.bulutsantralim.com/";

    private readonly ILogger<VerimorService> _logger;
    private readonly ApplicationDbContext _applicationDbContext;
    private readonly HttpClient _httpClient;

    public VerimorService(HttpClient httpClient,
        ApplicationDbContext applicationDbContext,
        ILogger<VerimorService> logger)
    {
        _httpClient = httpClient;
        _applicationDbContext = applicationDbContext;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 2, DelaysInSeconds = new[] { 60, 120 })]
    public async Task Call(Guid invitationId)
    {
        var invitation = await _applicationDbContext.Invitations.FirstOrDefaultAsync(x => x.Id == invitationId);

        if (invitation is null)
        {
            _logger.LogInformation("Invitation Not Found, InvitationId: {InvitationId}", invitationId);
            return;
        }

        var studentId = invitation.StudentId;
        var coachId = invitation.CoachId;

        var studentDetail = await _applicationDbContext.StudentDetail.Where(x => x.StudentId == studentId)
            .Select(x => new
            {
                FullName = x.Name + " " + x.Surname,
                Mobile = x.Mobile
            }).FirstOrDefaultAsync();

        var coachDetail = await _applicationDbContext.CoachDetail.Where(x => x.CoachId == coachId)
            .Select(x => new
            {
                FullName = x.Name + " " + x.Surname,
                Mobile = x.Mobile
            }).FirstOrDefaultAsync();
        
        var studentMobile = studentDetail.Mobile;
        var coachMobile = coachDetail.Mobile;

        if (string.IsNullOrEmpty(studentMobile) || string.IsNullOrEmpty(coachMobile))
        {
            _logger.LogInformation($"Student Mobile: {studentMobile} or Coach Mobile: {coachMobile} is empty. "+"{InvitationId}, StudentId: {StudentId}, CoachId: {StudentId}", invitationId, studentId, coachId);
            return;
        }

        coachMobile = coachMobile.Trim('-');
        studentMobile = studentMobile.Trim('-');

        //https://api.bulutsantralim.com/bridge?
        //key=Kd7df1cb1-d071-4222-9284-f2bed58d4818
        //&source=905352268341
        //&destination=905435998411
        //&recording_enabled=false
        var response = await _httpClient.GetAsync(apiBulutSantral +
                                                  $"bridge?key=Kd7df1cb1-d071-4222-9284-f2bed58d4818&source=90{coachMobile}&destination=90{studentMobile}&recording_enabled=false");
        
        //response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        
        VerimorCallDetail verimorCallDetail = null;
        
        if (Guid.TryParse(content, out var callId))
        {
            Thread.Sleep(TimeSpan.FromSeconds(50));
            // var callDetail = await _httpClient.GetAsync(apiBulutSantral +
            //                                           $"cdrs/{callId.ToString()}/?key=Kd7df1cb1-d071-4222-9284-f2bed58d4818");
            // https://api.bulutsantralim.com/cdrs/d6c7ea5a-08c2-450f-908a-a625fd56c688?key=Kd7df1cb1-d071-4222-9284-f2bed58d4818
            var verimorCallDetailResponse = await _httpClient.GetAsync(apiBulutSantral +
                                                                       $"cdrs/{callId.ToString()}/?key=Kd7df1cb1-d071-4222-9284-f2bed58d4818");

            if (verimorCallDetailResponse.IsSuccessStatusCode)
            {
                await using var stream = await verimorCallDetailResponse.Content.ReadAsStreamAsync();

                verimorCallDetail = await JsonSerializer.DeserializeAsync<VerimorCallDetail>(stream);    
            }
        }
        
        var sourceSide = verimorCallDetail?.call_flow?.FirstOrDefault(x => x.destination_number.Contains(coachMobile));
        var destinationSide = verimorCallDetail?.call_flow?.FirstOrDefault(x => x.destination_number.Contains(studentMobile));
        var sourceSideResult = sourceSide?.result ?? string.Empty;
        var destinationSideResult = destinationSide?.result ?? string.Empty;
        var cdrResult = verimorCallDetail?.cdr?.result ?? string.Empty;

        var isOk = !
                    (!response.IsSuccessStatusCode ||
                   (!string.IsNullOrEmpty(cdrResult) && !cdrResult.Equals("Cevaplandı")) ||
                   sourceSideResult.Equals("Vazgeçildi") ||
                   destinationSideResult.Equals("Vazgeçildi"));
        
        await _applicationDbContext.VoiceCallsHistories.AddAsync(new VoiceCallsHistory()
        {
            InvitationId = invitationId,
            Source = coachMobile,
            Destination = studentMobile,
            Content = content,
            Status = response.StatusCode.ToString(),
            CallDetailResult = verimorCallDetail?.cdr?.result,
            SourceResult = sourceSideResult,
            DestinationResult = destinationSideResult,
            Pair = coachDetail.FullName + " " + studentDetail.FullName,
            Ok = isOk,
            CreatedAt = DateTime.UtcNow,
        });

        await _applicationDbContext.SaveChangesAsync();
        
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(
                $"Calling Service Failed!, coachMobile: {coachMobile}, studentMobile: {studentMobile}, cdrResult: {cdrResult}, coachResult: {sourceSideResult}, studentResult: {destinationSideResult}");
        }
        
        if ((!string.IsNullOrEmpty(cdrResult) && !cdrResult.Equals("Cevaplandı")))
        {
            throw new Exception(
                $"Calling Service Failed!, coachMobile: {coachMobile}, studentMobile: {studentMobile}, cdrResult: {cdrResult}, coachResult: {sourceSideResult}, studentResult: {destinationSideResult}");
        }

        if (sourceSideResult.Equals("Vazgeçildi"))
        {
            throw new Exception(
                $"Calling Service Failed!, coachMobile: {coachMobile}, studentMobile: {studentMobile}, cdrResult: {cdrResult}, coachResult: {sourceSideResult}, studentResult: {destinationSideResult}");
        }
        
        if (destinationSideResult.Equals("Vazgeçildi"))
        {
            throw new Exception(
                $"Calling Service Failed!, coachMobile: {coachMobile}, studentMobile: {studentMobile}, cdrResult: {cdrResult}, coachResult: {sourceSideResult}, studentResult: {destinationSideResult}");
        }
    }
    
    [AutomaticRetry(Attempts = 2, DelaysInSeconds = new[] { 60, 120 })]
    public async Task CallTest(string sourceMobile, string destinationMobile, string pair)
    {
        // var retryCount = _performContext?.GetJobParameter<int>("RetryCount") ?? -1;
        
        _logger.LogInformation("SourceMobile: {SourceMobile}, DestinationMobile: {DestinationMobile}", sourceMobile, destinationMobile);
        
        //https://api.bulutsantralim.com/bridge?
        //key=Kd7df1cb1-d071-4222-9284-f2bed58d4818
        //&source=905352268341
        //&destination=905435998411
        //&recording_enabled=false
        var response = await _httpClient.GetAsync(apiBulutSantral +
                                                  $"bridge?key=Kd7df1cb1-d071-4222-9284-f2bed58d4818&source=90{sourceMobile}&destination=90{destinationMobile}&recording_enabled=false");
        
        //response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        
        VerimorCallDetail verimorCallDetail = null;
        
        if (Guid.TryParse(content, out var callId))
        {
            Thread.Sleep(TimeSpan.FromSeconds(50));
            // var callDetail = await _httpClient.GetAsync(apiBulutSantral +
            //                                           $"cdrs/{callId.ToString()}/?key=Kd7df1cb1-d071-4222-9284-f2bed58d4818");
            // https://api.bulutsantralim.com/cdrs/d6c7ea5a-08c2-450f-908a-a625fd56c688?key=Kd7df1cb1-d071-4222-9284-f2bed58d4818
            var verimorCallDetailResponse = await _httpClient.GetAsync(apiBulutSantral +
                                                                             $"cdrs/{callId.ToString()}/?key=Kd7df1cb1-d071-4222-9284-f2bed58d4818");

            if (verimorCallDetailResponse.IsSuccessStatusCode)
            {
                await using var stream = await verimorCallDetailResponse.Content.ReadAsStreamAsync();

                verimorCallDetail = await JsonSerializer.DeserializeAsync<VerimorCallDetail>(stream);    
            }
        };
        
        var sourceSide = verimorCallDetail?.call_flow?.FirstOrDefault(x => x.destination_number.Contains(sourceMobile));
        var destinationSide = verimorCallDetail?.call_flow?.FirstOrDefault(x => x.destination_number.Contains(destinationMobile));
        var sourceSideResult = sourceSide?.result ?? string.Empty;
        var destinationSideResult = destinationSide?.result ?? string.Empty;
        var cdrResult = verimorCallDetail?.cdr?.result ?? string.Empty;

        var isOk = !
                    (!response.IsSuccessStatusCode ||
                   (!string.IsNullOrEmpty(cdrResult) && !cdrResult.Equals("Cevaplandı")) ||
                   sourceSideResult.Equals("Vazgeçildi") ||
                   destinationSideResult.Equals("Vazgeçildi"));
        
        await _applicationDbContext.VoiceCallsHistories.AddAsync(new VoiceCallsHistory()
        {
            Source = sourceMobile,
            Destination = destinationMobile,
            Content = content,
            Status = response.StatusCode.ToString(),
            CallDetailResult = verimorCallDetail?.cdr?.result,
            SourceResult = sourceSideResult,
            DestinationResult = destinationSideResult,
            Pair = pair,
            Ok = isOk,
            CreatedAt = DateTime.UtcNow,
        });

        await _applicationDbContext.SaveChangesAsync();
        
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(
                $"Calling Service Failed!, sourceMobile: {sourceMobile}, destinationMobile: {destinationMobile}, cdrResult: {cdrResult}, sourceResult: {sourceSideResult}, destionationResult: {destinationSideResult}");
        }
        
        if ((!string.IsNullOrEmpty(cdrResult) && !cdrResult.Equals("Cevaplandı")))
        {
            throw new Exception(
                $"Calling Service Failed!, sourceMobile: {sourceMobile}, destinationMobile: {destinationMobile}, cdrResult: {cdrResult}, sourceResult: {sourceSideResult}, destionationResult: {destinationSideResult}");
        }

        if (sourceSideResult.Equals("Vazgeçildi"))
        {
            throw new Exception(
                $"Calling Service Failed!, sourceMobile: {sourceMobile}, destinationMobile: {destinationMobile}, cdrResult: {cdrResult}, sourceResult: {sourceSideResult}, destionationResult: {destinationSideResult}");
        }
        
        if (destinationSideResult.Equals("Vazgeçildi"))
        {
            throw new Exception(
                $"Calling Service Failed!, sourceMobile: {sourceMobile}, destinationMobile: {destinationMobile}, cdrResult: {cdrResult}, sourceResult: {sourceSideResult}, destionationResult: {destinationSideResult}");
        }
    }
    
    public class Cdr
    {
        public string direction { get; set; }
        public string caller_id_number { get; set; }
        public string destination_number { get; set; }
        public string result { get; set; }
        public string sip_hangup_disposition { get; set; }
        public bool missed { get; set; }
        public object return_uuid { get; set; }
        public string call_uuid { get; set; }
        public string start_stamp { get; set; }
        public string answer_stamp { get; set; }
        public string end_stamp { get; set; }
        public string duration { get; set; }
        public string talk_duration { get; set; }
        public string recording_present { get; set; }
    }

    public class CallFlow
    {
        public string destination_number { get; set; }
        public string start_stamp { get; set; }
        public string answer_stamp { get; set; }
        public string end_stamp { get; set; }
        public string duration { get; set; }
        public string ip_address { get; set; }
        public string sip_user_agent { get; set; }
        public string write_codec { get; set; }
        public string read_codec { get; set; }
        public string result { get; set; }
    }

    public class VerimorCallDetail
    {
        public Cdr cdr { get; set; }
        public List<CallFlow> call_flow { get; set; }
    }
}