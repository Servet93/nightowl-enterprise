﻿using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using NightOwlEnterprise.Api.Endpoints.Coachs.Identity;
using NightOwlEnterprise.Api.Endpoints.Coachs.Me;
using NightOwlEnterprise.Api.Endpoints.Coachs.Me.Students;
using NightOwlEnterprise.Api.Entities;

namespace NightOwlEnterprise.Api.Endpoints.Coachs;

public static class CoachIdentityEndpoints
{
    public static IEndpointConventionBuilder MapCoachsIdentityApi<TUser>(this IEndpointRouteBuilder endpoints)
        where TUser : class, new()
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        
        var emailSender = endpoints.ServiceProvider.GetRequiredService<IEmailSender<TUser>>();
        var linkGenerator = endpoints.ServiceProvider.GetRequiredService<LinkGenerator>();
        var coachConfig = endpoints.ServiceProvider.GetRequiredService<IOptions<CoachConfig>>()?.Value;
        var pdrConfig = endpoints.ServiceProvider.GetRequiredService<IOptions<PdrConfig>>()?.Value;
        var stripeCredential = endpoints.ServiceProvider.GetRequiredService<IOptions<StripeCredential>>()?.Value;
        var stripeCredentialSigningSecret = stripeCredential?.SigningSecret;
        
        var jwtHelper = endpoints.ServiceProvider.GetRequiredService<JwtHelper>();
        //var jwtConfig = endpoints.ServiceProvider.GetRequiredService<IOptions<JwtConfig>>()?.Value;
        
        var routeGroup = endpoints.MapGroup("coachs");

        routeGroup.MapRegister((IEmailSender<ApplicationUser>)emailSender,
            linkGenerator);
        routeGroup.MapForgotPassword((IEmailSender<ApplicationUser>)emailSender);
        routeGroup.MapResetPassword<ApplicationUser>();

        routeGroup.MapList();
        routeGroup.MapOnboard();
        routeGroup.MapQuota(coachConfig, pdrConfig);
        routeGroup.MapReserveCoach();
        routeGroup.MapReservationDays();
        routeGroup.MapMeReservationDays();
        routeGroup.MapMeReserveDayForStudent();
        routeGroup.MapInvitationDetailList();
        routeGroup.MapSpecifyHour();
        routeGroup.MapManageInfo();
        routeGroup.MapStateInfo();
        routeGroup.MapStudents();
        routeGroup.MapProgram();
        routeGroup.MapGetProfilePhoto();
        routeGroup.MapUploadProfilePhoto();
        

        return new IdentityEndpointsConventionBuilder(routeGroup);
    }
}
