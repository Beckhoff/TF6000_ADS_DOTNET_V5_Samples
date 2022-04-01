using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using TwinCAT;
using TwinCAT.Ads;
using TwinCAT.Ads.Reactive;
using TwinCAT.Ads.TypeSystem;
using TwinCAT.TypeSystem;

namespace S80_Reactive
{
    class Program
    {
        static void Main(string[] args)
        {
            SubscribeReactiveValueChanges();
            SubscribeReactiveStateChanges();
            SubscribeReactiveSymbolChanges();
            SubscribeReactiveAnyType();

        }

        public static void SubscribeReactiveAnyType()
        {

            // To Test the Observer run a project on the local PLC System (Port 851)
            using (AdsClient client = new AdsClient())
            {
                // Connect to target
                client.Connect(new AmsAddress(AmsNetId.Local, 851));

                // Reactive Notification Handler
                var valueObserver = Observer.Create<ushort>(val =>
                {
                    Console.WriteLine(string.Format("Value: {0}", val.ToString()));
                }
                );

                // Turning ADS Notifications into sequences of Value Objects (Taking 20 Values)
                // and subscribe to them.
                IDisposable subscription = client.WhenNotification<ushort>("TwinCAT_SystemInfoVarList._TaskInfo.CycleCount", NotificationSettings.Default).Take(20).Subscribe(valueObserver);

                Console.ReadKey(); // Wait for Key press
                subscription.Dispose(); // Dispose the Subscription
            }

            // To Test the Observer run a project on the local PLC System (Port 851)
            using (AdsClient client = new AdsClient())
            {
                // Connect to target
                client.Connect(new AmsAddress(AmsNetId.Local, 851));

                // Create Symbol information
                int instancesCount = 2;
                string[] instancePaths = new string[instancesCount];
                Type[] instanceTypes = new Type[instancesCount];
                object[] tags = new object[instancesCount];


                instancePaths[0] = "TwinCAT_SystemInfoVarList._TaskInfo.CycleCount";
                instanceTypes[0] = typeof(ushort);
                tags[0] = instancePaths[0];

                instancePaths[1] = "TwinCAT_SystemInfoVarList._TaskInfo.LastExecTime";
                instanceTypes[1] = typeof(uint);
                tags[1] = instancePaths[1];

                // Reactive Notification Handler
                var valueObserver = Observer.Create<ValueNotification>(val =>
                {
                    Console.WriteLine(string.Format("Handle: {0}, Value: {1}, Tag: {2}", val.Handle, val.ToString(), val.UserData));
                }
                );

                // Turning ADS Notifications into sequences of Value Objects (Taking 20 Values)
                // and subscribe to them.

                IDisposable subscription = client.WhenNotification(instancePaths,instanceTypes,NotificationSettings.Default,tags).Take(20).Subscribe(valueObserver);

                Console.ReadKey(); // Wait for Key press
                subscription.Dispose(); // Dispose the Subscription
            }

            // To Test the Observer run a project on the local PLC System (Port 851)
            using (AdsClient client = new AdsClient())
            {
                // Connect to target
                client.Connect(new AmsAddress(AmsNetId.Local, 851));

                // Create Symbol information
                var symbolLoader = SymbolLoaderFactory.Create(client, SymbolLoaderSettings.Default);
                IValueSymbol cycleCount = (IValueSymbol)symbolLoader.Symbols["TwinCAT_SystemInfoVarList._TaskInfo.CycleCount"];

                // Reactive Notification Handler
                var valueObserver = Observer.Create<object>(val =>
                {
                    Console.WriteLine(string.Format("Instance: {0}, Value: {1}", cycleCount.InstancePath, val.ToString()));
                }
                );

                // Take 20 Values in an Interval of 500ms
                IDisposable subscription = cycleCount.PollValues(TimeSpan.FromMilliseconds(500)).Take(20).Subscribe(valueObserver);

                Console.ReadKey(); // Wait for Key press
                subscription.Dispose(); // Dispose the Subscription
            }

            using (AdsClient client = new AdsClient())
            {
                // Connect to target
                client.Connect(new AmsAddress(AmsNetId.Local, 851));

                // Create Symbol information (Symbol 'i : INT' in PLC Global Variables list.
                var symbolLoader = SymbolLoaderFactory.Create(client, SymbolLoaderSettings.Default);
                IValueSymbol gvlIntSymbol = (IValueSymbol)symbolLoader.Symbols["GVL.i"];

                // Produces object (short) Values 0,1,2,3 ... in seconds period
                IObservable<object> timerObservable = Observable.Interval(TimeSpan.FromSeconds(1.0)).Select(i => (object)(short)i);

                // Take 10 Values (0..9) and write them to GVL.i
                IDisposable dispose = gvlIntSymbol.WriteValues(timerObservable.Take(10));

                Console.ReadKey(); // Wait for Key press
                dispose.Dispose(); // Dispose the Subscription
            }
        }

        public static void SubscribeReactiveStateChanges()
        {
            // To Test the observer, Start/Stop the local PLC

            using (AdsClient client = new AdsClient())
            {
                // Connect to target
                client.Connect(new AmsAddress(AmsNetId.Local, 851));

                // Create Symbol information
                var symbolLoader = SymbolLoaderFactory.Create(client, SymbolLoaderSettings.DefaultDynamic);

                // Reactive Notification Handler
                var valueObserver = Observer.Create<IList<AdsState>>(not =>
                    {
                        AdsState oldValue = not[0];
                        AdsState newValue = not[1];

                        Console.WriteLine(string.Format("Changed ADSState from '{0}' --> '{1}!", oldValue, newValue));
                    }
                );

                // Create a subscription for the AdsState change and buffering 2 Values (for oldValue --> newValue output).
                IDisposable subscription = client.WhenAdsStateChanges().Buffer(2,1).Subscribe(valueObserver);

                Console.ReadKey(); // Wait for Key press
                subscription.Dispose(); // Dispose the Subscription
            }

            // To Test the observer, Start/Stop the local PLC
            using (AdsClient client = new AdsClient())
            {
                // Connect to target
                client.Connect(new AmsAddress(AmsNetId.Local, 851));

                // Create Symbol information
                var symbolLoader = SymbolLoaderFactory.Create(client, SymbolLoaderSettings.DefaultDynamic);

                // Reactive Change Handler
                var valueObserver = Observer.Create<IList<AdsState>>(not =>
                {
                    AdsState oldValue = not[0];
                    AdsState newValue = not[1];

                    Console.WriteLine(string.Format("Changed ADSState from '{0}' --> '{1}!", oldValue, newValue));
                }
                );

                // Create a subscription for the AdsState change and buffering 2 Values (for oldValue --> newValue output).
                IDisposable subscription = client.PollAdsState(TimeSpan.FromMilliseconds(200)).Buffer(2, 1).Subscribe(valueObserver);

                Console.ReadKey(); // Wait for Key press
                subscription.Dispose(); // Dispose the Subscription
            }
        }

        public static void SubscribeReactiveSymbolChanges()
        {

            // To Test the Observer run a project on the local PLC System (Port 851)

            using (AdsClient client = new AdsClient())
            {
                // Connect to target
                client.Connect(new AmsAddress(AmsNetId.Local, 851));

                // Create Symbol information
                var symbolLoader = SymbolLoaderFactory.Create(client, SymbolLoaderSettings.Default);

                int eventCount = 1;

                // Reactive Notification Handler
                var valueObserver = Observer.Create<SymbolValueNotification>(not =>
                    {
                        Console.WriteLine(string.Format("{0} {1:u} {2} = '{3}' ({4})", eventCount++, not.TimeStamp, not.Symbol.InstancePath, not.Value, not.Symbol.DataType));
                    }
                );

                // Collect the symbols that are registered as Notification sources for their changed values.

                SymbolCollection notificationSymbols = new SymbolCollection();
                IArrayInstance taskInfo = (IArrayInstance)symbolLoader.Symbols["TwinCAT_SystemInfoVarList._TaskInfo"];
                
                foreach(ISymbol element in taskInfo.Elements)
                {
                    ISymbol cycleCount = element.SubSymbols["CycleCount"];
                    ISymbol lastExecTime = element.SubSymbols["LastExecTime"];

                    notificationSymbols.Add(cycleCount);
                    notificationSymbols.Add(lastExecTime);
                }

                // Create a subscription for the first 200 Notifications on Symbol Value changes.
                IDisposable subscription = client.WhenNotification(notificationSymbols,NotificationSettings.Default).Take(200).Subscribe(valueObserver);

                Console.ReadKey(); // Wait for Key press
                subscription.Dispose(); // Dispose the Subscription
            }

            // To Test the Observer run a project on the local PLC System (Port 851)
            using (AdsClient client = new AdsClient())
            {
                // Connect to target
                client.Connect(new AmsAddress(AmsNetId.Local, 851));

                // Create Symbol information
                var symbolLoader = SymbolLoaderFactory.Create(client, SymbolLoaderSettings.Default);
                IValueSymbol cycleCount = (IValueSymbol)symbolLoader.Symbols["TwinCAT_SystemInfoVarList._TaskInfo[1].CycleCount"];

                // Reactive Notification Handler
                var valueObserver = Observer.Create<object>(val =>
                {
                    Console.WriteLine(string.Format("Instance: {0}, Value: {1}", cycleCount.InstancePath, val.ToString()));
                }
                );

                cycleCount.NotificationSettings = new NotificationSettings(AdsTransMode.OnChange, 500, 5000); // optional: Change NotificationSettings on Symbol

                // Turning ADS Notifications into sequences of Value Objects (Taking 20 Values)
                // and subscribe to them.
                IDisposable subscription = cycleCount.WhenValueChanged().Take(20).Subscribe(valueObserver);

                Console.ReadKey(); // Wait for Key press
                subscription.Dispose(); // Dispose the Subscription
            }

            // To Test the Observer run a project on the local PLC System (Port 851)
            using (AdsClient client = new AdsClient())
            {
                // Connect to target
                client.Connect(new AmsAddress(AmsNetId.Local, 851));

                // Create Symbol information
                var symbolLoader = (IDynamicSymbolLoader)SymbolLoaderFactory.Create(client, SymbolLoaderSettings.DefaultDynamic);
                dynamic symbols = symbolLoader.SymbolsDynamic;
                dynamic cycleCount = symbols.TwinCAT_SystemInfoVarList._TaskInfo[1].CycleCount;

                // Reactive Notification Handler
                var valueObserver = Observer.Create<object>(val =>
                {
                    // Value objects can be dynamically (on the fly) created objects here (e.g. structs)
                    Console.WriteLine(string.Format("Instance: {0}, Value: {1}", cycleCount.InstancePath, val.ToString()));
                }
                );

                cycleCount.NotificationSettings = new NotificationSettings(AdsTransMode.OnChange, 500, 5000); // optional: Change NotificationSettings on Symbol

                // Turning ADS Notifications into sequences of Value Objects (Taking 20 Values)
                // and subscribe to them.
                // We have to give the 'hint' about IValueSymbol here, that the CLR finds the Extension Method 'WhenValueChanged' during runtime.
                IDisposable subscription = ((IValueSymbol)cycleCount).WhenValueChanged().Take(20).Subscribe(valueObserver);

                Console.ReadKey(); // Wait for Key press
                subscription.Dispose(); // Dispose the Subscription
            }

            // To Test the Observer run a project on the local PLC System (Port 851)
            using (AdsClient client = new AdsClient())
            {
                // Connect to target
                client.Connect(new AmsAddress(AmsNetId.Local, 851));

                // Create Symbol information
                var symbolLoader = SymbolLoaderFactory.Create(client, SymbolLoaderSettings.Default);
                IValueSymbol cycleCount = (IValueSymbol)symbolLoader.Symbols["TwinCAT_SystemInfoVarList._TaskInfo.CycleCount"]; // UShort Type
                IValueSymbol lastExecTime = (IValueSymbol)symbolLoader.Symbols["TwinCAT_SystemInfoVarList._TaskInfo.LastExecTime"]; // UInt Type

                SymbolCollection symbols = new SymbolCollection();
                symbols.Add(cycleCount);
                symbols.Add(lastExecTime);

                // Reactive Notification Handler
                var valueObserver = Observer.Create<object>(val =>
                {
                    Console.WriteLine(string.Format("Instance: {0}, Value: {1}", cycleCount.InstancePath, val.ToString()));
                }
                );

                cycleCount.NotificationSettings = new NotificationSettings(AdsTransMode.OnChange,500,5000); // optional: Change NotificationSettings on Symbol

                // Turning ADS Notifications into sequences of Value Objects (Taking 20 Values)
                // and subscribe to them.
                IDisposable subscription = client.WhenValueChanged(symbols).Take(20).Subscribe(valueObserver);

                Console.ReadKey(); // Wait for Key press
                subscription.Dispose(); // Dispose the Subscription
            }

            // To Test the Observer run a project on the local PLC System (Port 851)
            using (AdsClient client = new AdsClient())
            {
                // Connect to target
                client.Connect(new AmsAddress(AmsNetId.Local, 851));

                // Create Symbol information
                var symbolLoader = SymbolLoaderFactory.Create(client, SymbolLoaderSettings.Default);
                IValueSymbol cycleCount = (IValueSymbol)symbolLoader.Symbols["TwinCAT_SystemInfoVarList._TaskInfo[1].CycleCount"];

                // Reactive Notification Handler
                var valueObserver = Observer.Create<object>(val =>
                {
                    Console.WriteLine(string.Format("Instance: {0}, Value: {1}", cycleCount.InstancePath, val.ToString()));
                }
                );

                // Take 20 Values in an Interval of 500ms
                IDisposable subscription = cycleCount.PollValues(TimeSpan.FromMilliseconds(500)).Take(20).Subscribe(valueObserver);

                Console.ReadKey(); // Wait for Key press
                subscription.Dispose(); // Dispose the Subscription
            }

            using (AdsClient client = new AdsClient())
            {
                // Connect to target
                client.Connect(new AmsAddress(AmsNetId.Local, 851));

                // Create Symbol information (Symbol 'i : INT' in PLC Global Variables list.
                var symbolLoader = SymbolLoaderFactory.Create(client, SymbolLoaderSettings.Default);
                IValueSymbol cycleCount = (IValueSymbol)symbolLoader.Symbols["GVL.i"];

                // Produces object Values 0,1,2,3 ... in seconds period
                IObservable<object> timerObservable = Observable.Interval(TimeSpan.FromSeconds(1.0)).Select(i => (object)(short)i);

                // Take 10 Values (0..9) and write them to GVL.i
                IDisposable dispose = cycleCount.WriteValues(timerObservable.Take(10));

                Console.ReadKey(); // Wait for Key press
                dispose.Dispose(); // Dispose the Subscription
            }
        }


        public static void SubscribeReactiveValueChanges()
        {

            // To Test the Observer run a project on the local PLC System (Port 851)

            using (AdsClient client = new AdsClient())
            {
                // Connect to target
                client.Connect(new AmsAddress("172.17.62.105.1.1", 851));

                // Create Symbol information
                var symbolLoader = SymbolLoaderFactory.Create(client, SymbolLoaderSettings.DefaultDynamic);

                int eventCount = 1;

                // Reactive Notification Handler
                var valueObserver = Observer.Create<ValueChangedEventArgs>(args =>
                {
                    Console.WriteLine(string.Format("{0} {1:u} {2} = '{3}' ({4})", eventCount++, args.DateTime, args.Symbol.InstancePath, args.Value, args.Symbol.DataType.Name));
                }
                );

                // Collect the symbols that are registered as Notification sources for their changed values.

                SymbolIterator recursiveIterator = new SymbolIterator(symbolLoader.Symbols, true); // Iterate over all Symbols
                SymbolCollection allBoolean = new SymbolCollection(recursiveIterator.Where(symbol => (symbol.DataType != null) && (symbol.DataType.Name == "BOOL") && symbol.InstancePath.StartsWith("GVL.b")));

                IArrayInstance taskInfo = (IArrayInstance)symbolLoader.Symbols["TwinCAT_SystemInfoVarList._TaskInfo"];

                // Create a subscription for the first 120 Value changes on any Boolean Variable (but not faster than one per second, throttled).
                //IDisposable subscription = client.WhenValueChanged(allBoolean).Throttle(new TimeSpan(0,0,1)).Take(120).Subscribe(valueObserver);
                IDisposable subscription = client.WhenValueChangedAnnotated(allBoolean).Subscribe(valueObserver);

                Console.ReadKey(); // Wait for Key press
                subscription.Dispose(); // Dispose the Subscription
            }
        }
    }
}
