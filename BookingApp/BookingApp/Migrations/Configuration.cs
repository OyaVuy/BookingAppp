using BookingApp.Models;

namespace BookingApp.Migrations
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Validation;
    using System.Diagnostics;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<BookingApp.Models.BAContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;

            // if you want to debug seed method, uncomment these lines. 
            // run update-database from Package Manager Console to start debugger
            //if (System.Diagnostics.Debugger.IsAttached == false)
            //{
            //    System.Diagnostics.Debugger.Launch();
            //}

        }

        protected override void Seed(BookingApp.Models.BAContext context)
        {
            System.Diagnostics.Debug.WriteLine("\n__________________________________\nConfiguration.Seed() debug:\n");

            #region Creating Roles
            if (!context.Roles.Any(r => r.Name == "Admin"))
            {
                var store = new RoleStore<IdentityRole>(context);
                var manager = new RoleManager<IdentityRole>(store);
                var role = new IdentityRole { Name = "Admin" };

                manager.Create(role);
            }

            if (!context.Roles.Any(r => r.Name == "Manager"))
            {
                var store = new RoleStore<IdentityRole>(context);
                var manager = new RoleManager<IdentityRole>(store);
                var role = new IdentityRole { Name = "Manager" };

                manager.Create(role);
            }

            if (!context.Roles.Any(r => r.Name == "AppUser"))
            {
                var store = new RoleStore<IdentityRole>(context);
                var manager = new RoleManager<IdentityRole>(store);
                var role = new IdentityRole { Name = "AppUser" };

                manager.Create(role);
            }
            #endregion

            // First, we have to add independent entities, then call context.SaveChanges() and then add dependent...

            #region Adding Users

            context.AppUsers.AddOrUpdate(
                  p => p.FullName,
                  new AppUser() { FullName = "Admin Adminovic" }
             );

            context.AppUsers.AddOrUpdate(
                 p => p.FullName,
                 new AppUser() { FullName = "Zvezdana Menadzerovic" }
            );
            context.AppUsers.AddOrUpdate(
                p => p.FullName,
                new AppUser() { FullName = "Miljana Menadzerovic" }
           );

            context.AppUsers.AddOrUpdate(
                p => p.FullName,
                new AppUser() { FullName = "AppUser AppUserovic" }
            );

            ContextHelper.SaveChanges(context);

            // neautentikovanog korisnika ne pravimo, 
            // to je bilo ko, ko pristupi, a da nije u rolama?
            #endregion

            #region Associating users with roles
            var userStore = new UserStore<BAIdentityUser>(context);
            var userManager = new UserManager<BAIdentityUser>(userStore);

            var us = context.Users.FirstOrDefault();

            // adding data to AspNetUsers table  -> context.Users
            // model entiteta u toj tabeli je IndentityUser odnodno BAIdentityUser 
            if (!context.Users.Any(u => u.UserName == "admin"))
            {
                var _appUser = context.AppUsers.FirstOrDefault(a => a.FullName == "Admin Adminovic");

                var user = new BAIdentityUser()
                {
                    Id = "admin",
                    UserName = "admin",
                    Email = "admin@yahoo.com",
                    PasswordHash = BAIdentityUser.HashPassword("admin"),
                    appUserId = _appUser.Id // navigation
                };
                userManager.Create(user);
                userManager.AddToRole(user.Id, "Admin");
            }

            if (!context.Users.Any(u => u.UserName == "manager"))
            {
                var _appUser = context.AppUsers.FirstOrDefault(a => a.FullName == "Miljana Menadzerovic");
                var user = new BAIdentityUser()
                {
                    Id = "manager",
                    UserName = "manager",
                    Email = "manager@yahoo.com",
                    PasswordHash = BAIdentityUser.HashPassword("manager"),
                    appUserId = _appUser.Id // navigation
                };
                userManager.Create(user);
                userManager.AddToRole(user.Id, "Manager");

            }

            if (!context.Users.Any(u => u.UserName == "appu"))
            {
                var _appUser = context.AppUsers.FirstOrDefault(a => a.FullName == "AppUser AppUserovic");
                var user = new BAIdentityUser()
                {
                    Id = "appu",
                    UserName = "appu",
                    Email = "appu@yahoo.com",
                    PasswordHash = BAIdentityUser.HashPassword("appu"),
                    appUserId = _appUser.Id // navigation
                };
                userManager.Create(user);
                userManager.AddToRole(user.Id, "AppUser");
            }

            ContextHelper.SaveChanges(context);
            #endregion

            #region Adding Countries, Regions and Places

            var countries = new List<Country>()
            {
                new Country(){Name="Serbia",Code="SRB"},
                new Country(){Name="Macedonia",Code="MCD"},
                new Country(){Name="Australia",Code="ASTRL"},
                new Country(){Name="Norway",Code="NRWY"},
                new Country(){Name="Cuba",Code="CBA"}
            };
            context.Countries.AddOrUpdate(c => c.Code, countries.ToArray());
            ContextHelper.SaveChanges(context); // moramo da uradimo da bi se generisao Id


            var regions = new List<Region>()
            {
                new Region(){Name="Backa", CountryId=countries[0].Id},
                new Region(){Name="Srem", CountryId=countries[0].Id},
                new Region(){Name="Banat", CountryId=countries[0].Id},

                new Region(){Name="Vardar", CountryId=countries[1].Id},

                new Region(){Name="New South Wales", CountryId=countries[2].Id},
                new Region(){Name="Victoria", CountryId=countries[2].Id},

                new Region(){Name="Hedmark", CountryId=countries[3].Id},
                new Region(){Name="Vestfold", CountryId=countries[3].Id},

                new Region(){Name="La Habana", CountryId=countries[4].Id},
            };

            context.Regions.AddOrUpdate(r => new { r.Name, r.CountryId }, regions.ToArray());
            ContextHelper.SaveChanges(context);

            // ovo ne moramo raditi, jer se automatski dodele regioni kad im dodelimo id drzave
            // countries[0].Regions.Add(regions[0]); // i tako za sve ostale regione 


            // ako regione dodamo kao child objekte na objekat koji je vec u bazi, u ovom slucaju 
            // sledeci put kad uradimo SaveChanges, oni ce se dodati u odgovarajucu tabelu u bazi
            // to znaci da ako imamo country C1 i region R1, ako je C1 vec dodata u bazu,
            // mozemo da uradimo C1.Regions.Add(R1) ili context.Regions.Add(R1)


            var places = new List<Place>()
            {
                new Place(){Name="Temerin", RegionId=regions[0].Id}, // backa
                new Place(){Name="Novi Sad", RegionId=regions[0].Id},

                new Place(){Name="Sremski Karlovci", RegionId=regions[1].Id}, // srem

                new Place(){Name="Zrenjanin", RegionId=regions[2].Id}, // banat


                new Place(){Name="Veles", RegionId=regions[3].Id}, // vardar


                new Place(){Name="Sydney", RegionId=regions[4].Id}, //NSW

                new Place(){Name="Melbourne", RegionId=regions[5].Id}, // victoria


                new Place(){Name="Hamar", RegionId=regions[6].Id}, // hedmark

                new Place(){Name="Tensberg", RegionId=regions[7].Id}, // vestfold


                new Place(){Name="Havana", RegionId=regions[8].Id}, // la habana
            };

            context.Places.AddOrUpdate(p => new { p.Name, p.RegionId }, places.ToArray());
            ContextHelper.SaveChanges(context);
            // sada su automatski povezani mesta sa odgovarajucim regionima, tj. region.places vise nije prazno
            // ne moramo mi to rucno da popunjavamo

            // nakon ovako dodatih podataka, ako izbrises countries (delete from table dbo.Countries u sql menageru), brisu se i regije i places
            // nisam probala kroz entity framework api da brisem i modifikujem...

            #endregion

            #region Adding AccomodationTypes and Accomodations

            var accTypes = new List<AccommodationType>()
            {
                new AccommodationType(){Name="Hotel"},
                new AccommodationType(){Name="Hostel"},
                new AccommodationType(){Name="Private Home"},
                new AccommodationType(){Name="Boutique hotel"},
                new AccommodationType(){Name="Cottage"}
            };
            context.AccomodationTypes.AddOrUpdate(at => at.Name, accTypes.ToArray());
            ContextHelper.SaveChanges(context);


            /*Ovako, buduci da OwnerId treba da bude Id postojeceg usera, pretpostavljam menadzera, ja sam pokusala da
             naprvim vise usera sa rolom menadzera, ali mi to nesto nije poslo za rukom, jer su oni nesot komplikovali da je username=roleid, svasta nesto
             pa svuda stavljam jednog istog ownera
             
            Uglavnom, moramo videti sta cemo za te usere, nemam sad zivaca da se batrgam oko toga
             */

            var owner = context.Users.Where(u => u.Id == "manager").FirstOrDefault();

            var accoms = new List<Accomodation>()
            {
                // novi sad
                new Accomodation(){ Name="Hotel Park", AccomodationTypeId=accTypes[0].Id,PlaceId=places[1].Id, OwnerId=owner.appUserId},
                new Accomodation(){ Name="Hotel Prezident", AccomodationTypeId=accTypes[0].Id,PlaceId=places[1].Id, OwnerId=owner.appUserId},

                new Accomodation(){ Name="City Hostel", AccomodationTypeId=accTypes[1].Id,PlaceId=places[1].Id, OwnerId=owner.appUserId},

                // karlovci
                new Accomodation(){ Name="Premier Prezident", AccomodationTypeId=accTypes[0].Id,PlaceId=places[2].Id,OwnerId=owner.appUserId},
                new Accomodation(){ Name="Apartman Zeravica", AccomodationTypeId=accTypes[2].Id,PlaceId=places[2].Id, OwnerId=owner.appUserId},

                // sidnej :) 
                new Accomodation(){ Name="Hotel Hilton", AccomodationTypeId=accTypes[0].Id,PlaceId=places[5].Id, OwnerId=owner.appUserId},
                new Accomodation(){ Name="The Glebe", AccomodationTypeId=accTypes[2].Id,PlaceId=places[5].Id, OwnerId=owner.appUserId},

                // hamar
                new Accomodation(){ Name="Iceland Air", AccomodationTypeId=accTypes[0].Id,PlaceId=places[7].Id, OwnerId=owner.appUserId},
                new Accomodation(){ Name="Scandic Hamar", AccomodationTypeId=accTypes[0].Id,PlaceId=places[7].Id,OwnerId=owner.appUserId},

                // havana
                new Accomodation(){ Name="Inglaterra Hotel", AccomodationTypeId=accTypes[0].Id,PlaceId=places[9].Id, OwnerId=owner.appUserId},
            };

            // ne moze na istom mestu dva smestaja da se isto zovu 
            context.Accomodations.AddOrUpdate(acom => new { acom.Name, acom.PlaceId }, accoms.ToArray());
            ContextHelper.SaveChanges(context);
            #endregion

            #region AddingRooms

            #endregion

            #region AddingRoomReservations and comments
            #endregion

        }
    }
}
