"use strict";

var connection = null;

document.getElementById("chatContainer").style.visibility = "hidden";
document.getElementById("sendButton").disabled = true;

// Aktif olan kişiyi izlemek için değişken tanımla
let activeUser = null;
let activeUserId = null;
let meUserId = null;

// Kişi listesine tıklama olayını dinle
document.getElementById("userList").addEventListener("click", function(event) {
    // Eğer tıklanan öğe bir list item ise
    if (event.target.tagName === "LI") {
        // Önceki aktif kişiyi işaretleme kaldır
        if (activeUser) {
            activeUser.classList.remove("active");
        }
        // Yeni aktif kişiyi işaretle
        activeUser = event.target;
        activeUser.classList.add("active");
        activeUserId = event.target.dataset.userId;
        clearChatHistory();

        var accessToken = document.getElementById("accessToken").value;
        
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

document.getElementById("startConnectionButton").addEventListener("click", function (event) {
    var accessToken = document.getElementById("accessToken").value;

    connection = startConnection(accessToken);

    fillUsers(accessToken);
    
    fillMeId(accessToken);
    
    event.preventDefault();
});

function startConnection(accessToken)
{
    var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub", {
        accessTokenFactory: () => {
            // Get and return the access token.
            // This function can return a JavaScript Promise if asynchronous
            // logic is required to retrieve the access token.
            return accessToken;
        }
    }).build();
    
    connection.start().then(function () {
        
        document.getElementById("sendButton").disabled = false;
        document.getElementById("accessTokenContainer").style.visibility = "hidden";
        document.getElementById("chatContainer").style.visibility = "visible";

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
            
            addMessage(meUserId == senderId, message, timestamp);

            var messagesList = document.getElementById("messagesList");
            
            //kullanıcı bilinçli bir şekilde scroll downı en aşağı çekmişse
            if (userSelectTheScrollDown){
                console.log("mesaj geldi scroll down edildi")
                messagesList.scrollTop = messagesList.scrollHeight;
            }
        });
        
    }).catch(function (err) {
        return console.error(err.toString());
    });
    
    return connection;
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

function fillUsers(accessToken){
    // Header'da Bearer token gönder
    const headers = {
        'Authorization': `Bearer ${accessToken}`
    };
    // API'den kişi listesini çekmek için bir GET isteği yap
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
            return console.error(err.toString());
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

            var accessToken = document.getElementById("accessToken").value;
            
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
