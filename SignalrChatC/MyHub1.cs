using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;

namespace SignalrChatC
{
    public class ChatHub : Hub
    {
        private static int numUsers = 0;

        private static Dictionary<string, UserState> usersDict = new Dictionary<string, UserState>();




        // when the client emits 'new message', this listens and executes
        public void NewMessage(string data) {
            Clients.Others.newMessage(new MessageModel() {
              Username = Clients.Caller.userName,
              Message = data
            });
        }

        // when the client emits 'add user', this listens and executes
        public void AddUser(string username) {
            Clients.Caller.userName = username;

            usersDict.Add(Context.ConnectionId, new UserState()
            {
                Username = username,
                 UserAdded = true
            });
            ++numUsers;

            //socket
            Clients.Caller.login(new { 
                numUsers = numUsers 
            });

            //socket.broadcast
            Clients.Others.userJoined(new { 
                username = Clients.Caller.userName,
                numUsers = numUsers 
            });
        }


         // when the client emits 'typing', we broadcast it to others
        public void Typing() {
            Clients.Others.typing(new
            {
                username = Clients.Caller.userName
            });
        }

        // when the client emits 'stop typing', we broadcast it to others
        public void StopTyping () {
            Clients.Others.stopTyping(new
            {
                username = Clients.Caller.userName
            });

        }

  // when the user disconnects.. perform this

        // Chatroom

        // usernames which are currently connected to the chat




        public override Task OnConnected()
        {
            Clients.Caller.addedUser = false;
            
            //Debug.WriteLine(Clients.Caller.addedUser);

            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            string name = Context.User.Identity.Name;


            UserState currUserState;
            usersDict.TryGetValue(Context.ConnectionId, out currUserState);


    
             // remove the username from global usernames list
            if (currUserState != null)
            {
              --numUsers;
                
              // echo globally that this client has left
              Clients.Others.userLeft(new
              {
                    username = currUserState.Username,
                    numUsers =  numUsers
              });
              //delete usernames[socket.username];
              usersDict.Remove(Context.ConnectionId);

            }


            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            string name = Context.User.Identity.Name;
           
           // if (!_connections.GetConnections(name).Contains(Context.ConnectionId))
          //  {
          //      _connections.Add(name, Context.ConnectionId);
          //  }

            return base.OnReconnected();
        }


    }


    public class MessageModel
    {
        // We declare Left and Top as lowercase with 
        // JsonProperty to sync the client and server models
        [JsonProperty("username")]
        public string Username { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
    }

    public class UserState
    {
        public string Username { get; set; }
        public bool UserAdded { get; set; }
    }

}