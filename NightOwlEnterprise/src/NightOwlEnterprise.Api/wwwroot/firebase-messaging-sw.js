// Import the functions you need from the SDKs you need
importScripts("https://www.gstatic.com/firebasejs/10.12.2/firebase-app.js", "https://www.gstatic.com/firebasejs/10.12.2/firebase-messaging.js");

// import { initializeApp } from "https://www.gstatic.com/firebasejs/10.12.2/firebase-app.js";
// import { getToken, getMessaging, onMessage, onBackgroundMessage } from 'https://www.gstatic.com/firebasejs/10.12.2/firebase-messaging.js'
// import { onBackgroundMessage } from 'https://www.gstatic.com/firebasejs/10.12.2/firebase-messaging-sw.js'
// TODO: Add SDKs for Firebase products that you want to use
// https://firebase.google.com/docs/web/setup#available-libraries

// Your web app's Firebase configuration
const firebaseConfig = {
    apiKey: "AIzaSyCCAnJUTrtKmZQ8a0DhucxuaqW5eVU4XaM",
    authDomain: "nightowlenterprise-7a6c9.firebaseapp.com",
    projectId: "nightowlenterprise-7a6c9",
    storageBucket: "nightowlenterprise-7a6c9.appspot.com",
    messagingSenderId: "864372115977",
    appId: "1:864372115977:web:03418a0ab80ba32b2d86d8"
};

// Initialize Firebase
const firebaseApp  = initializeApp(firebaseConfig);
console.log(firebaseApp );

// Retrieve an instance of Firebase Messaging so that it can handle background
// messages.
const messaging = getMessaging(firebaseApp);

// Add the public key generated from the console here. 
// getToken(messaging, {vapidKey: "BKagOny0KF_2pCJQ3m....moL0ewzQ8rZu"}); 
getToken(messaging, { vapidKey: 'BG8ZPBU7X7lnasY_Tm-VISROjf6K_u84baTYN705IureHKjj2LfaxkzFDPWAg91f5fC-BaGJg8mkeyDEbr3aaL4' }).then((currentToken) => {
    if (currentToken) {
        console.log("currentToken -> " + currentToken);
        // Send the token to your server and update the UI if necessary 
        // ... 
    } else {
        // Show permission request UI 
        console.log('No registration token available. Request permission to generate one.');
        // ... 
    }
}).catch((err) => {
    console.log('An error occurred while retrieving token. ', err);
    // ... 
});

onBackgroundMessage(messaging, (payload) => {
    console.log('[firebase-messaging-sw.js] Received background message ', payload);
    // Customize notification here
    const notificationTitle = 'Background Message Title';
    const notificationOptions = {
        body: 'Background Message body.',
        icon: '/firebase-logo.png'
    };

    self.registration.showNotification(notificationTitle,
        notificationOptions);
});
