// https://stackoverflow.com/questions/71242841/listen-to-keyboard-input-in-the-whole-blazor-page
var attached = false;
window.JsFunctions = {
    addKeyboardListeners: function () {
        let serializeEvent = function (e) {
            if (e) {
                return {
                    key: e.key,
                    code: e.keyCode.toString(),
                    location: e.location,
                    repeat: e.repeat,
                    ctrlKey: e.ctrlKey,
                    shiftKey: e.shiftKey,
                    altKey: e.altKey,
                    metaKey: e.metaKey,
                    type: e.type
                };
            }
        };

        if (!attached)
        {
            console.log("This function should only be invoked once!");
            window.document.addEventListener('keydown', function (e) {
                DotNet.invokeMethodAsync('MissionControl', 'JsKeyDown', serializeEvent(e))
            });
            window.document.addEventListener('keyup', function (e) {
                DotNet.invokeMethodAsync('MissionControl', 'JsKeyUp', serializeEvent(e))
            });
            attached = true;
        }
    }
};
