namespace iRacingSDK;

public enum ChatCommandMode
{
    /// <summary>
    /// pass in a number from 1-15 representing the chat macro to launch
    /// </summary>
    Macro = 0,
    /// <summary>
    /// Open up a new chat window
    /// </summary>
    BeginChat,
    /// <summary>
    /// Reply to last private chat
    /// </summary>
    Reply,
    /// <summary>
    /// Close chat window
    /// </summary>
    Cancel
};
