using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jacere.ConsoleCredentials;

namespace Jacere.ConsoleCommand.Credentials
{
    [ConsoleCommand("Credentials")]
    public class CredentialsCommand : IConsoleCommand
    {
        private static readonly Dictionary<string, Type> SupportedTypes = new Dictionary<string, Type>();

        public static void Use(string key, Type type)
        {
            SupportedTypes.Add(key, type);
        }

        [ConsoleCommandOption("list", "l")]
        public bool List { get; set; }

        [ConsoleCommandOption("create", "c")]
        public string Create { get; set; }

        [ConsoleCommandOption("delete", "d")]
        public string Delete { get; set; }

        [ConsoleCommandOption("destroy")]
        public bool Destroy { get; set; }

        [ConsoleCommandOption("create-password")]
        public bool CreatePassword { get; set; }

        [ConsoleCommandOption("change-password")]
        public bool ChangePassword { get; set; }

        public Task Execute()
        {
            if (CreatePassword)
            {
                CredentialStorage.CreatePassword();
            }
            else
            {
                var storage = CredentialStorage.Open();

                if (Destroy)
                {
                    storage.Destroy();
                }
                else if (ChangePassword)
                {
                    storage.UpdateSecretKey();
                }
                else
                {
                    if (Delete != null)
                    {
                        storage.Delete(Delete);
                    }

                    if (Create != null)
                    {
                        if (Create == null)
                        {
                            throw new Exception("unspecified type to create");
                        }

                        if (!SupportedTypes.ContainsKey(Create))
                        {
                            throw new Exception("invalid type to create");
                        }

                        var type = SupportedTypes[Create];
                        storage.Set(type);
                    }

                    if (List)
                    {
                        foreach (var name in storage.Get())
                        {
                            Console.WriteLine(name);
                        }
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}
