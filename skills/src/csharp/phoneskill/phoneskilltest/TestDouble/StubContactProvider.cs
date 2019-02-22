using System.Collections.Generic;
using System.Threading.Tasks;
using PhoneSkill.Common;
using PhoneSkill.Models;

namespace PhoneSkillTest.TestDouble
{
    public class StubContactProvider : IContactProvider
    {
        private IList<ContactCandidate> contacts;

        public StubContactProvider()
        {
            contacts = new List<ContactCandidate>();

            var andrewSmith = new ContactCandidate();
            andrewSmith.Name = "Andrew Smith";
            var andrewSmithHome = new PhoneNumber();
            andrewSmithHome.Number = "555 111 1111";
            andrewSmithHome.Type.Standardized = PhoneNumberType.StandardType.HOME;
            andrewSmith.PhoneNumbers.Add(andrewSmithHome);
            var andrewSmithBusiness = new PhoneNumber();
            andrewSmithBusiness.Number = "555 222 2222";
            andrewSmithBusiness.Type.Standardized = PhoneNumberType.StandardType.BUSINESS;
            andrewSmith.PhoneNumbers.Add(andrewSmithBusiness);
            var andrewSmithMobile = new PhoneNumber();
            andrewSmithMobile.Number = "555 333 3333";
            andrewSmithMobile.Type.Standardized = PhoneNumberType.StandardType.MOBILE;
            andrewSmith.PhoneNumbers.Add(andrewSmithMobile);
            contacts.Add(andrewSmith);

            var andrewJones = new ContactCandidate();
            andrewJones.Name = "Andrew Jones";
            var andrewJonesBusiness = new PhoneNumber();
            andrewJonesBusiness.Number = "555 444 4444";
            andrewJonesBusiness.Type.Standardized = PhoneNumberType.StandardType.BUSINESS;
            andrewJones.PhoneNumbers.Add(andrewJonesBusiness);
            var andrewJonesMobile = new PhoneNumber();
            andrewJonesMobile.Number = "555 555 5555";
            andrewJonesMobile.Type.Standardized = PhoneNumberType.StandardType.MOBILE;
            andrewJones.PhoneNumbers.Add(andrewJonesMobile);
            contacts.Add(andrewJones);

            var bob = new ContactCandidate();
            bob.Name = "Bob Botter";
            var bobHome = new PhoneNumber();
            bobHome.Number = "555 666 6666";
            bobHome.Type.Standardized = PhoneNumberType.StandardType.HOME;
            bob.PhoneNumbers.Add(bobHome);
            contacts.Add(bob);

            var christinaRodriguez = new ContactCandidate();
            christinaRodriguez.Name = "Christina Rodriguez";
            contacts.Add(christinaRodriguez);

            var christinaSanchez = new ContactCandidate();
            christinaSanchez.Name = "Christina Sanchez";
            contacts.Add(christinaSanchez);

            var dithaNarthwani = new ContactCandidate();
            dithaNarthwani.Name = "Ditha Narthwani";
            var dithaNarthwaniHome = new PhoneNumber();
            dithaNarthwaniHome.Number = "555 777 7777";
            dithaNarthwaniHome.Type.Standardized = PhoneNumberType.StandardType.MOBILE;
            dithaNarthwani.PhoneNumbers.Add(dithaNarthwaniHome);
            contacts.Add(dithaNarthwani);

            var sanjayNarthwani = new ContactCandidate();
            sanjayNarthwani.Name = "Sanjay Narthwani";
            var sanjayNarthwaniHome = new PhoneNumber();
            sanjayNarthwaniHome.Number = "555 888 8888";
            sanjayNarthwaniHome.Type.Standardized = PhoneNumberType.StandardType.MOBILE;
            sanjayNarthwani.PhoneNumbers.Add(sanjayNarthwaniHome);
            contacts.Add(sanjayNarthwani);
        }

        public Task<IList<ContactCandidate>> GetContacts()
        {
            return Task.FromResult<IList<ContactCandidate>>(contacts);
        }
    }
}
