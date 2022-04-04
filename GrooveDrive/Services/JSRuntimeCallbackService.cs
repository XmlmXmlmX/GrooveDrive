using Microsoft.JSInterop;
using System.Text.Json;

namespace GrooveDrive.Services;

public class JSRuntimeCallbackResponse
{
    public string[] Arguments { get; private set; }

    public JSRuntimeCallbackResponse(string[] arguments)
    {
        this.Arguments = arguments;
    }

    public T? GetArg<T>(int i)
    {
        return JsonSerializer.Deserialize<T>(Arguments[i]);
    }
}

public class JSRuntimeCallbackService
{
    private readonly IJSRuntime _js;
    private readonly DotNetObjectReference<JSRuntimeCallbackService> _this;
    private Dictionary<string, Action<string[]>> callbacks = new ();

    public JSRuntimeCallbackService(IJSRuntime JSRuntime)
    {
        _js = JSRuntime;
        _this = DotNetObjectReference.Create(this);
    }

    [JSInvokable]
    public void Callback(string callbackId, string[] arguments)
    {
        if (callbacks.TryGetValue(callbackId, out Action<string[]>? callback))
        {
            callbacks.Remove(callbackId);
            callback(arguments);
        }
    }

    public Task<JSRuntimeCallbackResponse> InvokeJS(string cmd, params object[] args)
    {
        TaskCompletionSource<JSRuntimeCallbackResponse> t = new ();

        InvokeJS((string[] arguments) => {
            t.TrySetResult(new JSRuntimeCallbackResponse(arguments));
        }, cmd, args);
        return t.Task;
    }

    public void InvokeJS(Action<JSRuntimeCallbackResponse> callback, string cmd, params object[] args)
    {
        InvokeJS((string[] arguments) => {
            callback(new JSRuntimeCallbackResponse(arguments));
        }, cmd, args);
    }

    private void InvokeJS(Action<string[]> callback, string cmd, object[] args)
    {
        string callbackId;
        do
        {
            callbackId = Guid.NewGuid().ToString();
        } while (callbacks.ContainsKey(callbackId));
        callbacks[callbackId] = callback;
        _js.InvokeVoidAsync("GrooveDrive.Callback", _this, "Callback", callbackId, cmd, JsonSerializer.Serialize(args));
    }
}
