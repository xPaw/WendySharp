#region License
/*
NetIRC2
Copyright (c) 2002, 2013 James F. Bellinger <http://www.zer7.com>
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met: 

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer. 
2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution. 

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion

namespace NetIrc2
{
    /// <summary>
    /// Many of the common IRC error and reply codes.
    /// </summary>
    public enum IrcReplyCode
    {
        /// <summary>
        /// States that a nickname you specified does not exist on the server.
        /// </summary>
        NoSuchNickname = 401,

        /// <summary>
        /// States that a server you specified could be found.
        /// </summary>
        NoSuchServer = 402,

        /// <summary>
        /// States that a channel you specified could not be found.
        /// </summary>
        NoSuchChannel = 403,

        /// <summary>
        /// States that you are not allowed to send to a specific channel.
        /// </summary>
        CannotSendToChannel = 404,

        /// <summary>
        /// States that there are already too many channels on the server.
        /// </summary>
        TooManyChannels = 405,

        /// <summary>
        /// States that the nickname you asked about never existed.
        /// </summary>
        WasNoSuchNickname = 406,

        /// <summary>
        /// States that you specified too many targets for your message.
        /// </summary>
        TooManyTargets = 407,

        /// <summary>
        /// States that your ping/pong message did not have an origin parameter.
        /// </summary>
        NoOriginSpecified = 409,

        /// <summary>
        /// States that you did not specify a target for your message.
        /// </summary>
        NoRecipientGiven = 411,

        /// <summary>
        /// States that there was no text to send.
        /// </summary>
        NoTextToSend = 412,

        /// <summary>
        /// States that a message could not be delivered, because no top level
        /// domain name was specified. In other words, you sent a malformed message.
        /// </summary>
        NoTopLevelDomainSpecified = 413,

        /// <summary>
        /// States that a message could not be delivered, because there
        /// was a wildcard in the top level domain name you specified. In other
        /// words, you sent a malformed message.
        /// </summary>
        WildcardInTopLevelDomain = 414,

        /// <summary>
        /// States that the server does not understand your command.
        /// </summary>
        UnknownCommand = 421,

        /// <summary>
        /// States that the server's Message of the Day file is missing.
        /// </summary>
        MissingMOTD = 422,

        /// <summary>
        /// States that no administrative information could be found. This only
        /// occurs in response to a request for said information.
        /// </summary>
        NoAdminInfoAvailable = 423,

        /// <summary>
        /// States that an error occured when transferring a message. This is
        /// a fairly generic message, and does not necessarily mean you did
        /// anything incorrectly.
        /// </summary>
        FileError = 424,

        /// <summary>
        /// States that a nickname parameter was expected and was not received.
        /// </summary>
        NoNicknameGiven = 431,

        /// <summary>
        /// States that the nickname you specified contained invalid characters.
        /// </summary>
        ErroneousNickname = 432,

        /// <summary>
        /// States that the nickname you specified is already in use.
        /// </summary>
        NicknameInUse = 433,

        /// <summary>
        /// States that the nickname you specified is already in use, but on another
        /// server.
        /// </summary>
        NicknameCollision = 436,

        /// <summary>
        /// States that the nickname or channel is temporarily unavailable.
        /// </summary>
        ResourceUnavailable = 437,

        /// <summary>
        /// States that the user specified for a command pertaining to a specific
        /// channel is not in the channel.
        /// </summary>
        UserNotInChannel = 441,

        /// <summary>
        /// States that you are not in the channel you are trying to act upon.
        /// </summary>
        NotInChannel = 442,

        /// <summary>
        /// States that the user you invited to a channel is already in said channel.
        /// </summary>
        UserAlreadyInChannel = 443,

        /// <summary>
        /// States that the user specified could not be summoned, because they are
        /// not logged in.
        /// </summary>
        UserNotLoggedIn = 444,

        /// <summary>
        /// States that summoning is disabled.
        /// </summary>
        SummonCommandDisabled = 445,

        /// <summary>
        /// States that the 'USERS' command is disabled.
        /// </summary>
        UsersCommandDisabled = 446,

        /// <summary>
        /// States that you are not registered with the server, and registration
        /// is required for an action you attempted.
        /// </summary>
        HaveNotRegistered = 451,

        /// <summary>
        /// States that an IRC command lacked some parameters.
        /// </summary>
        NotEnoughParameters = 461,

        /// <summary>
        /// States that you may not reregister with the server.
        /// </summary>
        AlreadyRegistered = 462,

        /// <summary>
        /// States that you may not communicate with the server, because
        /// you do not have the appropriate priviledges.
        /// </summary>
        UnpriviledgedHost = 463,

        /// <summary>
        /// States that the password you specified to connect was invalid.
        /// </summary>
        IncorrectPassword = 464,

        /// <summary>
        /// States that the server has been set up to deny connections
        /// to your computer.
        /// </summary>
        BannedFromServer = 465,

        /// <summary>
        /// States that the channel key has already been set.
        /// </summary>
        ChannelKeyAlreadySet = 467,

        /// <summary>
        /// States that the requested channel cannot be joined because it is full.
        /// </summary>
        ChannelIsFull = 471,

        /// <summary>
        /// States that a mode character you sent was invalid.
        /// </summary>
        UnknownModeCharacter = 472,

        /// <summary>
        /// States that the channel could not be joined because it is invite-only.
        /// </summary>
        InviteOnlyChannel = 473,

        /// <summary>
        /// States that you are banned from the channel.
        /// </summary>
        BannedFromChannel = 474,

        /// <summary>
        /// States that the channel could not be joined because the specified
        /// channel key was incorrect.
        /// </summary>
        BadChannelKey = 475,

        /// <summary>
        /// States that the channel name contains invalid characters.
        /// </summary>
        BadChannelName = 479,

        /// <summary>
        /// States that the client does not have IRC operator priviledges,
        /// which are required for some command that was attempted.
        /// </summary>
        NotIRCOperator = 481,

        /// <summary>
        /// States that you are not a channel operator for a channel and
        /// tried to do an action that required that.
        /// </summary>
        NotChannelOperator = 482,

        /// <summary>
        /// States that you may not kill the server.
        /// </summary>
        CannotKillServer = 483,

        /// <summary>
        /// States that the server is not set up to allow operator
        /// commands from your host.
        /// </summary>
        HostCannotUseOperCommand = 491,

        /// <summary>
        /// States that a mode change on a nickname included an unknown flag.
        /// </summary>
        UnknownModeFlag = 501
    }
}
