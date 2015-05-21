using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Three.Net.Utils
{
    public abstract class Subscribable<T> where T : class
    {
        public abstract SubscriptionToken<T> Subscribe(Action<T> onFire, SubscriptionPriority priority = SubscriptionPriority.NORMAL, Action onCancel = null);

        //public void Filter<T1>(Action<T> onFire, SubscriptionPriority priority = SubscriptionPriority.NORMAL, Action onCancel = null)
        //    where T1 : class, T
        //{
        //    Subscribe((t) =>
        //    {
        //        if (t.GetType().IsSubclassOf(typeof(T1)))
        //        {
        //            onFire(t as T1);
        //        }
        //    }, priority, () =>
        //    {
        //        onCancel();
        //    });
        //}
    }

    public enum SubscriptionPriority
    {
        EARLIEST,
        EARLIER,
        EARLY,
        NORMAL,
        LATE,
        LATER,
        LATEST,
    }

    public class SubscriptionToken<T> where T : class
    {
        internal Trigger<T> output;
        internal PriorityAction<T> priorityAction;

        internal SubscriptionToken(Trigger<T> output, PriorityAction<T> priorityAction)
        {
            this.output = output;
            this.priorityAction = priorityAction;
        }

        public void CancelSubscription()
        {
            output.removedSubscriptions.Add(priorityAction);
        }
    }

    public sealed class Trigger<T> : Subscribable<T> where T : class
    {
        internal List<PriorityAction<T>> subscribedInputs = new List<PriorityAction<T>>();
        internal Queue<PriorityAction<T>> addedSubscriptions = new Queue<PriorityAction<T>>();
        internal HashSet<PriorityAction<T>> removedSubscriptions = new HashSet<PriorityAction<T>>();

        public override SubscriptionToken<T> Subscribe(Action<T> onFire, SubscriptionPriority priority, Action onCancel)
        {
            var priorityAction = new PriorityAction<T>(onFire, priority, onCancel);
            addedSubscriptions.Enqueue(priorityAction);
            var token = new SubscriptionToken<T>(this, priorityAction);
            return token;
        }

        public void Fire(T arg)
        {
            if (addedSubscriptions.Count > 0)
            {
                subscribedInputs.AddRange(addedSubscriptions);
                addedSubscriptions.Clear();
                subscribedInputs.Sort((a, b) => a.Priority.CompareTo(b.Priority));
            }

            if (subscribedInputs.Count > 0)
            {
                Cleanup();
                foreach (var priorityAction in subscribedInputs)
                {
                    if (!removedSubscriptions.Contains(priorityAction))
                        priorityAction.OnFire(arg);
                }
                Cleanup();
            }
        }

        private void Cleanup()
        {
            foreach (var action in removedSubscriptions)
            {
                subscribedInputs.Remove(action);
                if (action.OnCancel != null) action.OnCancel();
            }
        }

        public void ClearAllSubscriptions()
        {
            foreach (var subscription in subscribedInputs) removedSubscriptions.Add(subscription);
            subscribedInputs.Clear();
            Cleanup();
        }
    }

    internal class PriorityAction<T>
    {
        public readonly SubscriptionPriority Priority;
        public readonly Action<T> OnFire;
        public readonly Action OnCancel;

        public PriorityAction(Action<T> onFire, SubscriptionPriority priority, Action onCancel)
        {
            OnFire = onFire;
            Priority = priority;
            OnCancel = onCancel;
        }
    }
}
