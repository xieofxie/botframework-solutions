using System.Collections.Generic;
using System.Threading.Tasks;
using PhoneSkill.Common;
using PhoneSkill.Models;

namespace PhoneSkillTest.TestDouble
{
    public class StubContactProvider : IContactProvider
    {
        public static readonly ContactCandidate AndrewSmith = new ContactCandidate
        {
            Name = "Andrew Smith",
            PhoneNumbers = new List<PhoneNumber>
            {
                new PhoneNumber
                {
                    Number = "555 111 1111",
                    Type = new PhoneNumberType
                    {
                        Standardized = PhoneNumberType.StandardType.HOME,
                    },
                },
                new PhoneNumber
                {
                    Number = "555 222 2222",
                    Type = new PhoneNumberType
                    {
                        Standardized = PhoneNumberType.StandardType.BUSINESS,
                    },
                },
                new PhoneNumber
                {
                    Number = "555 333 3333",
                    Type = new PhoneNumberType
                    {
                        Standardized = PhoneNumberType.StandardType.MOBILE,
                    },
                },
            },
        };

        public static readonly ContactCandidate AndrewJones = new ContactCandidate
        {
            Name = "Andrew Jones",
            PhoneNumbers = new List<PhoneNumber>
            {
                new PhoneNumber
                {
                    Number = "555 444 4444",
                    Type = new PhoneNumberType
                    {
                        Standardized = PhoneNumberType.StandardType.BUSINESS,
                    },
                },
                new PhoneNumber
                {
                    Number = "555 555 5555",
                    Type = new PhoneNumberType
                    {
                        Standardized = PhoneNumberType.StandardType.MOBILE,
                    },
                },
            },
        };

        public static readonly ContactCandidate BobBotter = new ContactCandidate
        {
            Name = "Bob Botter",
            PhoneNumbers = new List<PhoneNumber>
            {
                new PhoneNumber
                {
                    Number = "555 666 6666",
                    Type = new PhoneNumberType
                    {
                        Standardized = PhoneNumberType.StandardType.HOME,
                    },
                },
            },
        };

        public static readonly ContactCandidate ChristinaRodriguez = new ContactCandidate
        {
            Name = "Christina Rodriguez",
        };

        public static readonly ContactCandidate ChristinaSanchez = new ContactCandidate
        {
            Name = "Christina Sanchez",
        };

        public static readonly ContactCandidate DithaNarthwani = new ContactCandidate
        {
            Name = "Ditha Narthwani",
            PhoneNumbers = new List<PhoneNumber>
            {
                new PhoneNumber
                {
                    Number = "555 777 7777",
                    Type = new PhoneNumberType
                    {
                        Standardized = PhoneNumberType.StandardType.MOBILE,
                    },
                },
            },
        };

        public static readonly ContactCandidate SanjayNarthwani = new ContactCandidate
        {
            Name = "Sanjay Narthwani",
            PhoneNumbers = new List<PhoneNumber>
            {
                new PhoneNumber
                {
                    Number = "555 888 8888",
                    Type = new PhoneNumberType
                    {
                        Standardized = PhoneNumberType.StandardType.MOBILE,
                    },
                },
            },
        };

        public static readonly ContactCandidate EveSmith = new ContactCandidate
        {
            Name = "Eve Smith",
            PhoneNumbers = new List<PhoneNumber>
            {
                new PhoneNumber
                {
                    Number = "555 999 9999",
                    Type = new PhoneNumberType
                    {
                        Standardized = PhoneNumberType.StandardType.HOME,
                    },
                },
                new PhoneNumber
                {
                    Number = "555 101 0101",
                    Type = new PhoneNumberType
                    {
                        Standardized = PhoneNumberType.StandardType.MOBILE,
                    },
                },
                new PhoneNumber
                {
                    Number = "555 121 2121",
                    Type = new PhoneNumberType
                    {
                        Standardized = PhoneNumberType.StandardType.MOBILE,
                    },
                },
            },
        };

        private readonly IList<ContactCandidate> contacts = new List<ContactCandidate>
        {
            AndrewSmith,
            AndrewJones,
            BobBotter,
            ChristinaRodriguez,
            ChristinaSanchez,
            DithaNarthwani,
            SanjayNarthwani,
            EveSmith,
        };

        public Task<IList<ContactCandidate>> GetContacts()
        {
            return Task.FromResult<IList<ContactCandidate>>(contacts);
        }
    }
}
