using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace SOTM.MissionControl.Utilities
{

    public static class KeyboardEvents
    {
        private static List<Action<KeyboardEventArgs>> _keyDownHandlers = new();
        private static List<Action<KeyboardEventArgs>> _keyUpHandlers = new();

        public static void AddKeyDownListener(Action<KeyboardEventArgs> listener)
        {
            _keyDownHandlers.Add(listener);
            Console.WriteLine($"Added listener: {listener}");
        }
        public static void RemoveKeyDownListener(Action<KeyboardEventArgs> listener)
        {
            Console.WriteLine($"Removed listener: {_keyDownHandlers.Remove(listener)}");
        }

        public static void AddKeyUpListener(Action<KeyboardEventArgs> listener)
        {
            _keyUpHandlers.Add(listener);
        }
        public static void RemoveKeyUpListener(Action<KeyboardEventArgs> listener)
        {
            _keyUpHandlers.Remove(listener);
        }

        [JSInvokable]
        public static Task JsKeyDown(KeyboardEventArgs e)
        {
            foreach (Action<KeyboardEventArgs> listener in _keyDownHandlers)
            {
                listener.Invoke(e);
            }

            return Task.CompletedTask;
        }

        [JSInvokable]
        public static Task JsKeyUp(KeyboardEventArgs e)
        {
            foreach (Action<KeyboardEventArgs> listener in _keyUpHandlers)
            {
                listener.Invoke(e);
            }

            return Task.CompletedTask;
        }
    }
}
