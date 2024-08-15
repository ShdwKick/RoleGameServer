namespace Server.Data
{
    public class UserData
    {
        public Guid? id { get; set; }
        public string c_nickname { get; set; }
        public string c_email { get; set; }
        public string c_password { get; set; }
        public DateOnly d_registrationdate { get; set; }
        public Guid f_authorizationtoken { get; set; }
        public Guid f_role { get; set; }
        public bool b_emailconfirmed { get; set; }

        public UserData()
        {
            id = Guid.NewGuid();
        }
    }
    public class User
    {
        public Guid? id { get; set; }
        public string c_nickname { get; set; }
        public string c_email { get; set; }
        public DateOnly d_registrationdate { get; set; }
        public Roles f_role { get; set; }
        public bool b_emailconfirmed { get; set; }

        public User()
        {
            id = Guid.NewGuid();
        }
    }

    public class UserForCreate
    {
        public string c_nickname { get; set; }
        public string c_email { get; set; }
        public string c_password { get; set; }
    }
    
    public class Roles
    {
        public Guid? id { get; set; }
        public string c_name { get; set; }
        public string c_devname { get; set; }
        public string c_description { get; set; }
    }
    public class AuthorizationTokens
    {
        public Guid? id { get; set; }
        public string c_token { get; set; }

        [GraphQLIgnore]
        public string c_hashsum { get; set; }
        public AuthorizationTokens()
        {
            id = Guid.NewGuid();
        }
    }

    public class ChatsFilterWords
    {
        public Guid? id { get; set; }
        public string c_word { get; set; }
        public string c_correctedword { get; set; }
    }
    
    public class Message
    {
        public Guid? id { get; set; }
        public Guid f_sender { get; set; }
        public string c_content { get; set; }
        public DateTime d_datetime { get; set; }
        public Guid f_chat { get; set; }
    }


    public class Chats
    {
        public Guid? id { get; set; }
        public Guid? f_firstuser { get; set; }
        public Guid? f_seconduser { get; set; }
        public bool b_isglobalchat { get; set; }
    }
    public class DBItems
    {
        public Guid? id { get; set; }
        public Guid f_inventory { get; set; }
        public Guid f_item { get; set; }
        public int amount { get; set; }
    }

    public class Items
    {
        public Guid? id { get; set; }
        public Guid f_inventory { get; set; }
        public Item f_item { get; set; }
        public int amount { get; set; }
    }
    
    public class Item
    {
        public Guid? id { get; set; }
        public string c_name { get; set; }
        public string c_description { get; set; }
        public float n_price { get; set; }
    }
}