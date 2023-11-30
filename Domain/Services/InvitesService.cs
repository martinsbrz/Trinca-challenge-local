using CrossCutting;
using Domain.Entities;
using Domain.Events;
using Domain.Interfaces;
using Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Services
{
    internal class InvitesService : IInvitesService
    {
        public SnapshotStore _snapshots { get; set; }
        public IPersonRepository _peopleStore { get; set; }
        public IBbqRepository _bbqStore { get; set; }
        public InvitesService(SnapshotStore snapshots, IPersonRepository peopleStore, IBbqRepository bbqStore)
        {
            _snapshots = snapshots;
            _peopleStore = peopleStore;
            _bbqStore = bbqStore;
        }

        public async Task SendNewBbq(string Id, DateTime Date, string Reason)
        {
            var Lookups = await _snapshots.AsQueryable<Lookups>("Lookups").SingleOrDefaultAsync();
            foreach (var personId in Lookups.ModeratorIds)
            {
                var person = await _peopleStore.GetAsync(personId);
                var @event = new PersonHasBeenInvitedToBbq(Id, Date, Reason);

                person!.Apply(@event);

                await _peopleStore.SaveAsync(person);
            }
        }

        public async Task SendBbqResponse(Bbq Churras, bool GonnaHappen)
        {
            var lookups = await _snapshots.AsQueryable<Lookups>("Lookups").SingleOrDefaultAsync();

            foreach (var personId in lookups.PeopleIds)
            {
                var person = await _peopleStore.GetAsync(personId);
                var hasInvite = person!.Invites.Any(e => e.Id == Churras.Id);

                if (!GonnaHappen)
                {
                    if (hasInvite)
                    {
                        person.Apply(new InviteWasDeclined { InviteId = Churras.Id, PersonId = person.Id });
                    }
                }
                else
                {
                    if (!hasInvite)
                    {
                        person.Apply(new PersonHasBeenInvitedToBbq(Churras.Id, Churras.Date, Churras.Reason));
                    }
                }

                await _peopleStore.SaveAsync(person);
            }
        }

        public async Task AcceptBbqInvite(Person Person, string InviteId, bool IsVeg)
        {
            Person.Apply(new InviteWasAccepted { InviteId = InviteId, IsVeg = IsVeg, PersonId = Person.Id });
            await _peopleStore.SaveAsync(Person);

            //implementar efeito do aceite do convite no churrasco
            //quando tiver 7 pessoas ele está confirmado

            var bbq = new Bbq();
            bbq = await _bbqStore.GetAsync(InviteId);
            bbq.Apply(new InviteWasAccepted { InviteId = InviteId, IsVeg = IsVeg, PersonId = Person.Id });
            await _bbqStore.SaveAsync(bbq);
        }

        public async Task DeclineBbqInvite(Person Person, string InviteId)
        {
            Person.Apply(new InviteWasDeclined { InviteId = InviteId, PersonId = Person.Id });
            await _peopleStore.SaveAsync(Person);

            //Implementar impacto da recusa do convite no churrasco caso ele já tivesse sido aceito antes

            var bbq = new Bbq();
            bbq = await _bbqStore.GetAsync(InviteId);
            bbq.Apply(new InviteWasDeclined { InviteId = InviteId, PersonId = Person.Id });
            await _bbqStore.SaveAsync(bbq);
        }
    }
}
