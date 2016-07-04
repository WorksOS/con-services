using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using VSP.MasterData.Common.KafkaWrapper.Interfaces;

namespace VSP.MasterData.Common.KafkaWrapper
{
  public class EventAggregator : IEventAggregator //can be created as singleton
  {
    private readonly Dictionary<Type, List<WeakReference>> eventSubscriberList =
      new Dictionary<Type, List<WeakReference>>();

    private readonly object padlock = new object();

    public void Subscribe<TEvent>(ISubscriber<TEvent> subscriber)
    {
      lock (padlock)
      {
        IEnumerable<Type> subscriberTypes = subscriber.GetType().GetInterfaces()
          .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof (ISubscriber<>));
        var weakReference = new WeakReference(subscriber);
        foreach (Type subscriberType in subscriberTypes)
        {
          List<WeakReference> subscribers = GetSubscribers(subscriberType);
          subscribers.Add(weakReference);
        }
      }
    }


    public void ProcessMessage<TEvent>(TEvent eventToPublish)
    {
      //get subscribers for the type we publish
      Type subscriberType = typeof (ISubscriber<>).MakeGenericType(typeof (TEvent));
      List<WeakReference> subscribers = GetSubscribers(subscriberType);
      //to hold dead subscribers while iterating since we cant remove while iterating
      var subscribersToRemove = new List<WeakReference>();
      foreach (WeakReference weakSubscriber in subscribers)
      {
        if (!weakSubscriber.IsAlive)
        {
          subscribersToRemove.Add(weakSubscriber);
          continue;
        }
        var subscriber = (ISubscriber<TEvent>) weakSubscriber.Target;
        //marshall in case subscriber and publisher are in different threads
        SynchronizationContext syncContext = SynchronizationContext.Current ?? new SynchronizationContext();
        //syncContext.Post(s => subscriber.Handle(eventToPublish), null);    

        syncContext.Send(s => subscriber.Handle(eventToPublish), null);
      }

      if (subscribersToRemove.Any())
      {
        lock (padlock)
        {
          foreach (WeakReference remove in subscribersToRemove)
            subscribers.Remove(remove);
        }
      }
    }

    private List<WeakReference> GetSubscribers(Type subscriberType)
    {
      List<WeakReference> subscribers;
      lock (padlock)
      {
        bool found = eventSubscriberList.TryGetValue(subscriberType, out subscribers);
        if (!found)
        {
          subscribers = new List<WeakReference>();
          eventSubscriberList.Add(subscriberType, subscribers);
        }
      }
      return subscribers;
    }
  }
}