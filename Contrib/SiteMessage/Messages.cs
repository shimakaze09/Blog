namespace Contrib.SiteMessage;

public class Messages
{
    private Queue<Message> MessagesQueue { get; } = new();
    public bool IsEmpty => MessagesQueue.Count == 0;

    public Message Dequeue()
    {
        return MessagesQueue.Dequeue();
    }

    public void Debug(string content)
    {
        MessagesQueue.Enqueue(new Message
        {
            Tag = MessageTags.Debug,
            Content = content
        });
    }

    public void Success(string content)
    {
        MessagesQueue.Enqueue(new Message
        {
            Tag = MessageTags.Success,
            Content = content
        });
    }

    public void Info(string content)
    {
        MessagesQueue.Enqueue(new Message
        {
            Tag = MessageTags.Info,
            Content = content
        });
    }

    public void Warning(string content)
    {
        MessagesQueue.Enqueue(new Message
        {
            Tag = MessageTags.Warning,
            Content = content
        });
    }

    public void Error(string content)
    {
        MessagesQueue.Enqueue(new Message
        {
            Tag = MessageTags.Error,
            Content = content
        });
    }
}