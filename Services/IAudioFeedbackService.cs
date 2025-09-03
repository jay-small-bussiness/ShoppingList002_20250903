using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShoppingList002.Services
{
    public interface IAudioFeedbackService
    {
        void PlaySound(FeedbackType type);
    }

    public enum FeedbackType
    {
        Start,
        OneHitAdded,
        MultiHit,
        NoHit,
        AlreadyExists
    }

}
