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
        public Role FRole { get; set; }
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
    
    public class Role
    {
        public Guid? id { get; set; }
        public string c_name { get; set; }
        public string c_dev_name { get; set; }
        public string c_description { get; set; }
    }
    public class AuthorizationToken
    {
        public Guid? id { get; set; }
        public string c_token { get; set; }

        [GraphQLIgnore]
        public string c_hash { get; set; }
        public AuthorizationToken()
        {
            id = Guid.NewGuid();
        }
    }

    public class RecoveryCodes
    {
        public Guid id { get; set; }
        public string c_email { get; set; }
        public int n_code { get; set; }
        public DateTime d_expiration_time { get; set; }
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

    public class RoomChat
    {
        public Guid id { get; set; }
        public Guid? f_room_id { get; set; }
    }
    public class PrivateChat
    {
        public Guid id { get; set; }
        public Guid? f_firstuser { get; set; }
        public Guid? f_seconduser { get; set; }
    }

    public class PrivateChatUsers
    {
        public Guid id { get; set; }
        public Guid? f_chat_id { get; set; }
        public Guid? f_user_id { get; set; }
    }

    public class DBItems
    {
        public Guid id { get; set; }
        public Guid f_inventory { get; set; }
        public Guid f_item { get; set; }
        public int n_amount { get; set; }
    }

    public class Items
    {
        public Guid id { get; set; }
        public Guid f_inventory { get; set; }
        public Item f_item { get; set; }
        public int n_amount { get; set; }
    }
    
    public class Item
    {
        public Guid id { get; set; }
        public string c_name { get; set; }
        public string c_description { get; set; }
        public float n_price { get; set; }
    }

    public class Inventory
    {
        public Guid id { get; set; }
        public Guid? f_character_id { get; set; }
    }

    public class Game
    {
        public Guid id { get; set; }
        public string c_game_name { get; set; }

    }

    public class CreateRoom
    {
        public string c_name { get; set; }
        public string c_description { get; set; }
        public Guid? f_owner_id { get; set; }
    }

    public class Room
    {
        public Guid id { get; set; }
        public string c_name { get; set; }
        public string c_description { get; set; }
        public Guid? f_owner_id { get; set; }
        public int n_size { get; set; }
        [GraphQLIgnore]
        public Guid? f_game { get; set; }
    }

    public class RoomUsers
    {
        public Guid id { get; set; }
        public Guid? f_room_id { get; set; }
        public Guid? f_user_id { get; set; }
        public bool b_is_master { get; set; }
    }

    public class Stats
    {
        public Guid id { get; set; }
        public int n_strngth { get; set; }
        public int n_dexterity { get; set; }
        public int n_constitution { get; set; }
        public int n_intelligence { get; set; }
        public int n_wisdom { get; set; }
        public int n_charisma { get; set; }
    }
}