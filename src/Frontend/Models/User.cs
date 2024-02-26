using System;
using System.Text.Json;

namespace Frontend.Models
{
    public class User
    {
        public int ID { get; set; }
        public string Nickname { get; set; }

        private static User instance;
        public static User Instance {
            get {
                if (instance == null)
                    instance = new User();

                return instance;
            }
            set {
                instance = value;
            }
        }
    }
}

