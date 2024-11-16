namespace Web.Contrib.SiteMessage;

public class MessageService
{
    private const string DefaultTitle = "Prompt Information";
    private const string SessionKey = "message-service-id";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public MessageService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private Dictionary<string, Queue<Message>> MessageQueues { get; } = new();

    public HttpContext? HttpContext => _httpContextAccessor.HttpContext;

    public Queue<Message> CurrentQueue
    {
        get
        {
            if (HttpContext == null) throw new Exception("There is no active HttpContext.");

            var id = HttpContext.Session.GetString(SessionKey);
            if (id == null)
            {
                id = Guid.NewGuid().ToString();
                HttpContext.Session.SetString(SessionKey, id);
            }

            Queue<Message> queue;
            if (MessageQueues.TryGetValue(id, out var messageQueue))
            {
                queue = messageQueue;
            }
            else
            {
                queue = new Queue<Message>();
                MessageQueues[id] = queue;
            }

            return queue;
        }
    }

    public bool IsEmpty => CurrentQueue.Count == 0;

    public Message Dequeue()
    {
        return CurrentQueue.Dequeue();
    }

    public Message Enqueue(string tag, string title, string content)
    {
        var message = new Message
        {
            Tag = tag,
            Title = title,
            Content = content
        };
        CurrentQueue.Enqueue(message);

        return message;
    }

    public Message Debug(string content, string title = DefaultTitle)
    {
        return Enqueue(MessageTags.Debug, title, content);
    }

    public Message Success(string content, string title = DefaultTitle)
    {
        return Enqueue(MessageTags.Success, title, content);
    }

    public Message Info(string content, string title = DefaultTitle)
    {
        return Enqueue(MessageTags.Info, title, content);
    }

    public Message Warning(string content, string title = DefaultTitle)
    {
        return Enqueue(MessageTags.Warning, title, content);
    }

    public Message Error(string content, string title = DefaultTitle)
    {
        return Enqueue(MessageTags.Error, title, content);
    }
}