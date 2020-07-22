using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Errors;
using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Activities {
    public class Attend {
        public class Command : IRequest {
            public Guid Id { get; set; }
        }

        public class Handler : IRequestHandler<Command> {
            private readonly DataContext _context;
            private readonly IUserAccessor _userAccessor;
            public Handler (DataContext context, IUserAccessor userAccessor) {
                _userAccessor = userAccessor;
                _context = context;
            }

            public async Task<Unit> Handle (Command request, CancellationToken cancellationToken) {

                var activity = await _context.Activities.FindAsync (request.Id);

                if (activity == null)
                    throw new RestException (HttpStatusCode.NotFound, new { Activity = "Could not find activity" });

                // We can not use FindAsync because the Id we have 
                // is not the user's id
                var user = await _context.Users.SingleOrDefaultAsync (x => x.UserName == _userAccessor.GetCurrentUsername ());

                var attendance = await _context.UserActivities.SingleOrDefaultAsync (x =>
                    x.ActivityId == activity.Id && x.AppUserId == user.Id);

                // Checks if the user is already in the attendees for 
                // the specified activity
                if (attendance != null)
                    throw new RestException (HttpStatusCode.BadRequest,
                        new { Attendance = " Already attending this activity" });

                attendance = new UserActivity {
                    Activity = activity,
                    AppUser = user,
                    IsHost = false,
                    DateJoined = DateTime.Now
                };

                _context.UserActivities.Add (attendance);

                // 1) Save the changes into the database
                // 2) It verifies if it was successfully saved
                var success = await _context.SaveChangesAsync () > 0;

                if (success) return Unit.Value;

                throw new Exception ("Problem saving changes");
            }
        }
    }
}