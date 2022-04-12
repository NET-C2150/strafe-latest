﻿
using Sandbox;
using Sandbox.UI;

namespace Strafe.UI;

[UseTemplate]
internal partial class Chat : Panel
{

	public static Chat Current;

	public Panel Canvas { get; set; }
	public TextEntry Input { get; set; }

	public bool Open
	{
		get => HasClass( "open" );
		set => SetClass( "open", value );
	}

	public Chat()
	{
		Current = this;

		Input.AddEventListener( "onsubmit", Submit );
		Input.AddEventListener( "onblur", Close );
		Sandbox.Hooks.Chat.OnOpenChat += OnOpenChat;

		Canvas.PreferScrollToBottom = true;
	}

	private void OnOpenChat()
	{
		Open = true;
		Input.Focus();
		Input.Text = string.Empty;
	}

	private void Close()
	{
		Input.Text = string.Empty;
		Open = false;
	}

	private void Submit()
	{
		var msg = Input.Text;

		if ( string.IsNullOrWhiteSpace( msg ) ) return;

		if ( msg[0] == '/' )
		{
			StrafeGame.ServerCmd_ExecuteChatCommand( msg );
			return;
		}

		ChatBox.Say( msg );
	}

	public void AddEntry( string name, string message )
	{
		var msg = Canvas.AddChild<StrafeChatEntry>();
		msg.Name = name;
		msg.Message = message;

		Canvas.TryScrollToBottom();
	}

	[ClientCmd( "chat_add", CanBeCalledFromServer = true )]
	public static void AddChatEntry( string name, string message )
	{
		Current?.AddEntry( name, message );

		// Only log clientside if we're not the listen server host
		if ( !Global.IsListenServer )
		{
			Log.Info( $"{name}: {message}" );
		}
	}

	[ServerCmd( "say" )]
	public static void Say( string message )
	{
		Assert.NotNull( ConsoleSystem.Caller );

		// todo - reject more stuff
		if ( message.Contains( '\n' ) || message.Contains( '\r' ) )
			return;

		Log.Info( $"{ConsoleSystem.Caller}: {message}" );
		AddChatEntry( To.Everyone, ConsoleSystem.Caller.Name, message );
	}

}