using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IInvitesService
    {
        Task SendNewBbq(string Id, DateTime Date, string Reason);
        Task SendBbqResponse(Bbq Churras, bool GonnaHappen);
        Task AcceptBbqInvite(Person Person, string InviteId, bool IsVeg);
        Task DeclineBbqInvite(Person Person, string InviteId);
    }
}
