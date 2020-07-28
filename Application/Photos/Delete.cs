using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Errors;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Photos {
    public class Delete {
        public class Command : IRequest {
            public string Id { get; set; }
        }

        public class Handler : IRequestHandler<Command> {
            private readonly DataContext _context;
            private readonly IPhotoAccessor _photoAccessor;
            private readonly IUserAccessor _userAccessor;
            public Handler (DataContext context, IUserAccessor userAccessor, IPhotoAccessor photoAccessor) {
                _userAccessor = userAccessor;
                _photoAccessor = photoAccessor;
                _context = context;
            }

            public async Task<Unit> Handle (Command request, CancellationToken cancellationToken) {

                var user = await _context.Users.SingleOrDefaultAsync (x => x.UserName == _userAccessor.GetCurrentUsername ());

                var photo = user.Photos.FirstOrDefault (x => x.Id == request.Id);

                if (photo == null)
                    throw new RestException (HttpStatusCode.NotFound, new { Photo = "Not Found" });

                if (photo.IsMain)
                    throw new RestException (HttpStatusCode.BadRequest, new { Photo = "You can not delete your main photo" });

                var result = _photoAccessor.DeletePhoto (photo.Id);

                if (result == null)
                    throw new Exception ("Problem deleting the photo");

                user.Photos.Remove (photo);

                // 1) Save the changes into the database
                // 2) It verifies if it was successfully saved
                var success = await _context.SaveChangesAsync () > 0;

                if (success) return Unit.Value;

                throw new Exception ("Problem saving changes");
            }
        }
    }
}