/* --- BOT SETTINGS --- */
const botUrl = "https://942311285bf0.ngrok.io";
const chatIconMessageEs = "Consulta la agenda!";
/* --- /BOT SETTINGS --- */
//'#51397f', '#548c9b', '#a0c1b8',
const parameters = {
    style: {
        backgroundColor: '#FFFFFF', //window background color

        bubbleBackground: '#F1F1F4', //bot bubble background color
        bubbleTextColor: '#575A5E', //bot bubble text color
        botAvatarImage: 'https://1.bp.blogspot.com/-7UnZsiDpIac/X6htFmgCpjI/AAAAAAAAAE0/kGIWiO74lPIdnGqyXkHEaTSBwGB6wRK5gCLcBGAsYHQ/s1600/LogoW4TTnowords.png',
        botAvatarInitials: '', //needed to show image
        botAvatarBackgroundColor: '#FFFFFF',

        bubbleFromUserBackground: '#8A8A8A', //user bubble background color
        bubbleFromUserTextColor: '#ffffff', //user bubble text color

        suggestedActionBackground: 'White', //button background color
        suggestedActionBorderColor: '#cccccc', //button border color
        suggestedActionTextColor: '#06038D', //button text color

        //overlaybutton to move through carousel or suggested actions
        transcriptOverlayButtonBackground: '#d2dde5', //overlaybutton
        transcriptOverlayButtonBackgroundOnHover: '#ED823C',
        transcriptOverlayButtonColor: '#06038D',
        transcriptOverlayButtonColorOnHover: 'White' //parameter
    },
    maximize: {
        backgroundColor: '#2CCCD3',
        imageUrl: 'https://1.bp.blogspot.com/-7UnZsiDpIac/X6htFmgCpjI/AAAAAAAAAE0/kGIWiO74lPIdnGqyXkHEaTSBwGB6wRK5gCLcBGAsYHQ/s1600/LogoW4TTnowords.png'
    },
    header: {
        backgroundColor: '#06038D',
        color: '#FFFFFF',
        imageUrl: 'https://1.bp.blogspot.com/-7UnZsiDpIac/X6htFmgCpjI/AAAAAAAAAE0/kGIWiO74lPIdnGqyXkHEaTSBwGB6wRK5gCLcBGAsYHQ/s1600/LogoW4TTnowords.png',
        height: '40px'
    },
    directlineTokenUrl: botUrl + '/api/directline/generateToken/',
    directlineReconnectTokenUrl: botUrl + '/api/directline/reconnect/',
    //speechTokenUrl: botUrl + '/api/directline/speech/generatetoken/', //botframework-webchat: "authorizationToken", "region", and "subscriptionKey" are deprecated and will be removed on or after 2020-12-17. Please use "credentials" instead.
    //selectVoice: (voices, activity) => selectVoice(voices, activity),
    chatIconMessage: '¡Consulta la agenda!',
    language: 'es',
    locale: "es-ES"
}


function addscript(url) {
    var head = document.getElementsByTagName('head')[0];
    var scriptElement = document.createElement('script');
    scriptElement.setAttribute('src', url);

    head.appendChild(scriptElement);
    return scriptElement;
}

var scriptElement = addscript(botUrl + "/Scripts/main.js");

function addcss(url) {
    var head = document.getElementsByTagName('head')[0];
    var linkElement = document.createElement('link');
    linkElement.setAttribute('rel', 'stylesheet');
    linkElement.setAttribute('type', 'text/css');
    linkElement.setAttribute('href', botUrl + url);

    head.appendChild(linkElement);
}

addcss('/css/main.css');

function selectVoice(voices, activity) {
    return voices.find(({ lang, gender, name }) => lang.startsWith(activity.locale || parameters.language) && (/AlvaroNeural/iu.test(name) || /GuyNeural/iu.test(name)))
}

scriptElement.onload = function () {
    var scripts = document.getElementsByTagName('script');
    var arrScripts = Array.from(scripts);
    var myScript = arrScripts.find((e) => e.src.includes("botchat-launcher.js"));
    var queryString = myScript.src.replace(/^[^\?]+\??/, '');
    var jsParams = new URLSearchParams(queryString);

    if (jsParams.has('locale')) {
        parameters.language = jsParams.get('locale');
    }
    if (parameters.language.startsWith('en')) {
        parameters.chatIconMessage = chatIconMessageEs;
    }
    else {
        parameters.chatIconMessage = chatIconMessageEs;
    }

    window.Intelequia.renderApp('chat-bot', parameters);
};