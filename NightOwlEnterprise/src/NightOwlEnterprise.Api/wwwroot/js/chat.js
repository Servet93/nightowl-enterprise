"use strict";

var connection = null;

document.getElementById("chatContainer").style.display = "none";
document.getElementById("connectionStatus").style.display = "none";
document.getElementById("chatSessionContainer").style.display = "none";
document.getElementById("sendButton").disabled = true;

// Aktif olan kişiyi izlemek için değişken tanımla
let activeUser = null;
let accessToken = null;
let activeUserId = null;
let meUserId = null;

//first
document.getElementById('loginForm').addEventListener('submit', function(event) {
    event.preventDefault();
    fetch('/login', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            email: document.getElementById('email').value,
            password: document.getElementById('password').value
        })
    })
        .then(response => response.json())
        .then(data => {
            console.log(data);
            console.log(data.accessToken)
            if (data.accessToken) {
                accessToken = data.accessToken;
                alert('Login success.');
                document.getElementById("loginContainer").style.display = "none";
                document.getElementById("connectionStatus").style.display = "inline";
                var resultConnection = startConnection(data.accessToken);
                if (resultConnection == null){
                    alert('Chat connection couldnt be obtained.');
                }else{
                    connection = resultConnection;
                    
                    if (data.userType === 'Student'){
                        document.getElementById("userListTitle").text = "Mentörüm";
                        //Öğrenci ise koçu getir.
                        fillCoach(data.accessToken);
                        fillMeManageInfoForStudent(data.accessToken);
                    }else{
                        //Koç veya Pdr ise öğrencileri getir.
                        document.getElementById("userListTitle").text = "Öğrencilerim";
                        fillStudents(data.accessToken);    
                        fillMeManageInfoForCoach(data.accessToken);
                    }
                    // fillMeId(data.accessToken);
                }
            } else {
                alert('Login failed. Please check your credentials.');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            alert('Login failed. Error:' +  error);
        });
});


// Kişi listesine tıklama olayını dinle
document.getElementById("userList").addEventListener("click", function(event) {
    // Eğer tıklanan öğe bir list item ise
    if (event.target.tagName === "LI") {

        document.getElementById("chatSessionContainer").style.display = "block";
        
        // Önceki aktif kişiyi işaretleme kaldır
        if (activeUser) {
            activeUser.classList.remove("active");
        }
        // Yeni aktif kişiyi işaretle
        activeUser = event.target;
        activeUser.classList.add("active");
        activeUserId = event.target.dataset.userId;
        clearChatHistory();

        // var accessToken = document.getElementById("accessToken").value;
        
        const headers = {
            'Authorization': `Bearer ${accessToken}`
        };
        
        fetch('/chat/getMessages?limit=20' + '&receiverId=' + activeUserId,{
          method:"GET",
          headers: headers  
        })
            .then(response => response.json())
            .then(data => {
                // Gelen mesajları ekle
                data.forEach(message => {
                    // var li = document.createElement('li');
                    // li.textContent = message.content;
                    // li.setAttribute('data-message-date', message.timestamp);
                    // document.getElementById('messagesList').prepend(li);

                    addBeforeMessage(meUserId == message.senderId, message.content, message.timestamp);

                    var messagesList = document.getElementById("messagesList");
                    messagesList.scrollTop = messagesList.scrollHeight;
                    
                });
        });
    }
});

// document.getElementById("startConnectionButton").addEventListener("click", function (event) {
//     var accessToken = document.getElementById("accessToken").value;
//
//     connection = startConnection(accessToken);
//
//     fillUsers(accessToken);
//    
//     fillMeId(accessToken);
//    
//     event.preventDefault();startConnection
// });

function startConnection(accessToken)
{
    // /chatHub
    // var connection = new signalR.HubConnectionBuilder().withUrl("https://chat.baykusmentorluk.com/chatHub", {
    //     accessTokenFactory: () => {
    //         // Get and return the access token.
    //         // This function can return a JavaScript Promise if asynchronous
    //         // logic is required to retrieve the access token.
    //         return accessToken;
    //     }
    // }).build();

    // var connection = new signalR.HubConnectionBuilder().withUrl("http://localhost:5254/chatHub?access_token=" + accessToken, {
    var connection = new signalR.HubConnectionBuilder().withUrl("https://chat.baykusmentorluk.com/chatHub?access_token=" + accessToken, {
        fetchOptions: {
            credentials: 'omit',
        }
    }).build();
    
    connection.start().then(function () {
        
        updateConnectionStatus("connected");

        document.getElementById("chatContainer").style.display = "flex";
        document.getElementById("sendButton").disabled = false;
        // document.getElementById("accessTokenContainer").style.visibility = "hidden";

        connection.on("ReceiveMessage", function (senderId, receiverId, message, timestamp) {
            // console.log("connectionId -> " + connectionId);
            // console.log("userIdentifier -> " + userIdentifier);
            // console.log("additionalInfo -> " + additionalInfo);
            
            // var li = document.createElement("li");
            // document.getElementById("messagesList").appendChild(li);
            
            // We can assign user-supplied strings to an element's textContent because it
            // is not interpreted as markup. If you're assigning in any other way, you 
            // should be aware of possible script injection concerns.
            
            // if (meUserId == senderId){
            //     li.textContent = `me says -> ${message}`;    
            // }else{
            //     li.textContent = `${receiverId} says -> ${message}`;
            // }
            
            console.log("Receive Event -> ReceiverId: " + receiverId + ", SenderId: " + senderId + ", ActiveUserId : " + activeUserId);
            
            if (senderId === activeUserId || receiverId === activeUserId){
                addMessage(meUserId == senderId, message, timestamp);

                var messagesList = document.getElementById("messagesList");

                //kullanıcı bilinçli bir şekilde scroll downı en aşağı çekmişse
                if (userSelectTheScrollDown){
                    console.log("mesaj geldi scroll down edildi")
                    messagesList.scrollTop = messagesList.scrollHeight;
                }    
            }
        });

        // Bağlantı durumu izleme
        connection.onclose(function (error) {
            console.log("Bağlantı kapandı:", error);
            updateConnectionStatus("disconnected");
            alert("Connection Closed, Error: " + error);
        });

        connection.onreconnecting(function () {
            console.log("Yeniden bağlanılıyor...");
            updateConnectionStatus("reconnecting");
            alert("Connection is reconnectiong.");
        });

        connection.onreconnected(function (connectionId) {
            console.log("Yeniden bağlandı. Bağlantı Kimliği:", connectionId);
            updateConnectionStatus("connected");
            alert("Connection is recconected. ConnectionId: " + connectionId);
        });
        
    }).catch(function (err) {
        console.error(err.toString());
        updateConnectionStatus("disconnected");
        alert('Start Connection Failed. Error:' +  err.toString());
        return null;
    });
    
    return connection;
}

// Bağlantı durumunu güncelleyen yardımcı fonksiyon
function updateConnectionStatus(status) {
    var statusElement = $("#connectionStatus");

    // Duruma göre stil ve metin güncelle
    switch (status) {
        case "connected":
            statusElement.removeClass().addClass("connection-status connected");
            break;
        case "connecting":
            statusElement.removeClass().addClass("connection-status connecting");
            break;
        case "disconnected":
            statusElement.removeClass().addClass("connection-status disconnected");
            break;
        default:
            statusElement.removeClass().addClass("connection-status");
    }
}

function addMessage(isMine, content, timestamp) {
    var messageList = document.getElementById("messagesList");
    var li = document.createElement("li");
    li.className = "list-group-item-message";
    li.textContent = content;

    if (isMine) {
        li.classList.add("text-right");
    } else {
        li.classList.add("text-left");
    }

    li.setAttribute('data-message-date', timestamp);

    var timestampSpan = document.createElement("span");
    
    if (isMine){
        timestampSpan.className = "timestamp-right";    
    }else{
        timestampSpan.className = "timestamp-left";
    }
    
    timestampSpan.textContent = timestamp;

    li.appendChild(timestampSpan);
    messageList.appendChild(li);
}

function addBeforeMessage(isMine, content, timestamp) {
    var messageList = document.getElementById("messagesList");
    var li = document.createElement("li");
    li.className = "list-group-item-message";
    li.textContent = content;

    li.setAttribute('data-message-date', timestamp);
    
    if (isMine) {
        li.classList.add("text-right");
    } else {
        li.classList.add("text-left");
    }

    var timestampSpan = document.createElement("span");
    
    if (isMine){
        timestampSpan.className = "timestamp-right";
    }else{
        timestampSpan.className = "timestamp-left";
    }
    
    
    timestampSpan.textContent = timestamp;

    li.appendChild(timestampSpan);
    messageList.prepend(li);
}

function fillStudents(accessToken){
    // Header'da Bearer token gönder
    const headers = {
        'Authorization': `Bearer ${accessToken}`
    };

    // API'den kişi listesini çekmek için bir GET isteği yap -> 
    fetch('/coachs/me/students',{
        method: 'POST',
        headers: headers
    })
    .then(response => response.json())
    .then(data => {
        console.log(data);
        
        var studentItems = data.data;
        
        const userList = document.getElementById('userList');
        
        // Her kişi için bir liste öğesi oluştur ve HTML'e ekle
        studentItems.forEach(studentItem => {
            const listItem = document.createElement('li');
            listItem.textContent = studentItem.name + " " + studentItem.surname;
            listItem.dataset.userId = studentItem.id;
            listItem.classList.add('list-group-item');
            // Liste öğesine tıklanınca aktif olması için olay ekle
            listItem.addEventListener('click', function() {
                // Önceki aktif kişiyi işaretleme kaldır
                const previousActive = document.querySelector('.list-group-item.active');
                if (previousActive) {
                    previousActive.classList.remove('active');
                }
                // Yeni aktif kişiyi işaretle
                listItem.classList.add('active');
            });
            userList.appendChild(listItem);
        });
    })
    .catch(error => {
        console.error('Error fetching students: ', error);
        alert('Error fetching students: ' + error.toString());
    });
}

function fillCoach(accessToken){
    // Header'da Bearer token gönder
    const headers = {
        'Authorization': `Bearer ${accessToken}`
    };

    // API'den kişi listesini çekmek için bir GET isteği yap -> 
    fetch('/students/me/coach-info',{
        method: 'GET',
        headers: headers
    })
    .then(response => response.json())
    .then(data => {
        console.log(data);

        var coachInfo = data;

        const userList = document.getElementById('userList');

        const listItem = document.createElement('li');
        
        listItem.textContent = coachInfo.name + " " + coachInfo.surname;
        listItem.dataset.userId = coachInfo.id;
        listItem.classList.add('list-group-item');
        
        // Liste öğesine tıklanınca aktif olması için olay ekle
        listItem.addEventListener('click', function() {
            // Önceki aktif kişiyi işaretleme kaldır
            const previousActive = document.querySelector('.list-group-item.active');
            if (previousActive) {
                previousActive.classList.remove('active');
            }
            // Yeni aktif kişiyi işaretle
            listItem.classList.add('active');
        });
        userList.appendChild(listItem);
        
    })
    .catch(error => {
        console.error('Error fetching coach: ', error);
        alert('Error fetching coach: ' + error.toString());
    });
}

function fillUsers(accessToken, userType){
    // Header'da Bearer token gönder
    const headers = {
        'Authorization': `Bearer ${accessToken}`
    };
    
    // API'den kişi listesini çekmek için bir GET isteği yap -> 
    fetch('/chat/users',{
        method: 'GET',
        headers: headers
    })
    .then(response => response.json())
    .then(data => {
        const userList = document.getElementById('userList');
        // Her kişi için bir liste öğesi oluştur ve HTML'e ekle
        data.forEach(user => {
            const listItem = document.createElement('li');
            listItem.textContent = user.name;
            listItem.dataset.userId = user.id;
            listItem.classList.add('list-group-item');
            // Liste öğesine tıklanınca aktif olması için olay ekle
            listItem.addEventListener('click', function() {
                // Önceki aktif kişiyi işaretleme kaldır
                const previousActive = document.querySelector('.list-group-item.active');
                if (previousActive) {
                    previousActive.classList.remove('active');
                }
                // Yeni aktif kişiyi işaretle
                listItem.classList.add('active');
            });
            userList.appendChild(listItem);
        });
    })
    .catch(error => console.error('Error fetching users:', error));
}

function fillMeId(accessToken){
    // Header'da Bearer token gönder
    const headers = {
        'Authorization': `Bearer ${accessToken}`
    };
    // API'den kişi listesini çekmek için bir GET isteği yap
    fetch('/chat/meInfo',{
        method: 'GET',
        headers: headers
    })
        .then(response => response.json())
        .then(data => {
            const meIdInput = document.getElementById('meId');
            meIdInput.value = data.id;
            meUserId = data.id;
            
        })
        .catch(error => console.error('Error fetching users:', error));
}

function fillMeManageInfoForStudent(accessToken){
    // Header'da Bearer token gönder
    const headers = {
        'Authorization': `Bearer ${accessToken}`
    };
    // API'den kişi listesini çekmek için bir GET isteği yap
    fetch('/students/me/info',{
        method: 'GET',
        headers: headers
    })
    .then(response => response.json())
    .then(data => {
        const meIdInput = document.getElementById('meId');
        meIdInput.value = data.id;
        meUserId = data.id;
        console.log(data);
        document.getElementById('infoContainerTitle').innerText = "Öğrenci -> " + data.name + " " + data.surname;
    })
    .catch(error => console.error('Error fetching manage info for student:', error));
}

function fillMeManageInfoForCoach(accessToken){
    // Header'da Bearer token gönder
    const headers = {
        'Authorization': `Bearer ${accessToken}`
    };
    // API'den kişi listesini çekmek için bir GET isteği yap
    fetch('/coachs/me/info',{
        method: 'GET',
        headers: headers
    })
        .then(response => response.json())
        .then(data => {
            const meIdInput = document.getElementById('meId');
            meIdInput.value = data.id;
            meUserId = data.id;
            console.log(data);
            document.getElementById('infoContainerTitle').innerText = "Mentör -> " + data.name + " " + data.surname;
        })
        .catch(error => console.error('Error fetching manage info for coach:', error));
}

// Sohbet geçmişini temizleme fonksiyonu
function clearChatHistory() {
    const messagesList = document.getElementById('messagesList');
    messagesList.innerHTML = ''; // Tüm mesajları temizle
}

document.getElementById("sendButton").addEventListener("click", function (event) {
    // var user = document.getElementById("userInput").value;
    var message = document.getElementById("messageInput").value;
    
    if (message !== '' || message !== null){
        connection.invoke("SendMessage", activeUserId, message).then(function () {
            // Hata olmadığında yapılacak işlemler
            document.getElementById("messageInput").value = '';
            var messagesList = document.getElementById("messagesList");
            messagesList.scrollTop = messagesList.scrollHeight;
            console.log("Mesaj başarıyla gönderildi.");
        }).catch(function (err) {
            console.error(err.toString());
            alert("Message couldn't be sent to server, Error: " + err.toString());
        });
    }
    
    event.preventDefault();
});

// Sayfanın en altındaki mesaj tarihini al
function getLastMessageDate() {
    var messages = document.getElementById('messagesList').getElementsByTagName('li');
    if (messages.length > 0) {
        var lastMessage = messages[0];
        return lastMessage.getAttribute('data-message-date');
    }
    return null;
}

var userSelectTheScrollDown = false;
// Scroll olayını dinle
document.getElementById('messagesList').addEventListener('scroll', function() {
    var messagesList = this;

    //sayfanın en altına ulaşıldı.
    if (messagesList.scrollTop + messagesList.clientHeight >= messagesList.scrollHeight) {
        userSelectTheScrollDown = true;
        console.log("Sayfanın en altına ulaşıldı!");
    }else{
        userSelectTheScrollDown = false;
    }
    
    // Sayfanın en üstüne ulaşıldığında
    if (messagesList.scrollTop === 0) {
        console.log("scroll Top -> 0");
        // Son mesaj tarihini al
        var lastMessageDate = getLastMessageDate();
        if (lastMessageDate) {
            // AJAX veya fetch ile veri tabanından mesajları al
            // Örneğin:

            // var accessToken = document.getElementById("accessToken").value;
            
            const headers = {
                'Authorization': `Bearer ${accessToken}`
            };
            
            fetch('/chat/getMessages?lastMessageDate=' + lastMessageDate + '&receiverId=' + activeUserId, {
                method:"GET",
                headers: headers
            })
                .then(response => response.json())
                .then(data => {
                    // Gelen mesajları ekle
                    data.forEach(message => {
                        addBeforeMessage(meUserId == message.senderId, message.content, message.timestamp);
                        // var li = document.createElement('li');
                        // li.textContent = message.content;
                        // li.setAttribute('data-message-date', message.timestamp);
                        // document.getElementById('messagesList').prepend(li);
                    });
            });
        }
    }
});
