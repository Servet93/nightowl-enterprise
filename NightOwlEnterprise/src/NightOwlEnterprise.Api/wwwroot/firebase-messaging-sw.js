// Give the service worker access to Firebase Messaging.
// Note that you can only use Firebase Messaging here. Other Firebase libraries
// are not available in the service worker.
importScripts('https://www.gstatic.com/firebasejs/8.10.1/firebase-app.js');
importScripts('https://www.gstatic.com/firebasejs/8.10.1/firebase-messaging.js');

// Initialize the Firebase app in the service worker by passing in
// your app's Firebase config object.
// https://firebase.google.com/docs/web/setup#config-object
firebase.initializeApp({
    apiKey: "AIzaSyCCAnJUTrtKmZQ8a0DhucxuaqW5eVU4XaM",
    authDomain: "nightowlenterprise-7a6c9.firebaseapp.com",
    projectId: "nightowlenterprise-7a6c9",
    storageBucket: "nightowlenterprise-7a6c9.appspot.com",
    messagingSenderId: "864372115977",
    appId: "1:864372115977:web:03418a0ab80ba32b2d86d8"
});

// Retrieve an instance of Firebase Messaging so that it can handle background
// messages.
const messaging = firebase.messaging();

messaging.onBackgroundMessage((payload) => {
    console.log(
        '[firebase-messaging-sw.js] Received background message ',
        payload
    );
    // Customize notification here
    const notificationTitle = payload.notification.title;
    const notificationOptions = {
        body: payload.notification.body,
        icon: payload.notification.image
    };
    self.registration.showNotification(notificationTitle, notificationOptions);
});

// self.addEventListener("push", (event) => {
//     const notif = event.data.json().notification;
//    
//     event.waitUntil(self.registration.showNotification(notif.title, {
//         body: notif.body,
//         icon: notif.image,
//         data: {
//             url: notif.click_action
//         }
//     }));
// });
//
// self.addEventListener("notificationClick", (event) => {
//     event.waitUntil(clients.openWindow(event.notification.data.url));
// });

