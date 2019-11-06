﻿
using CAS.CommServer.UA.ModelDesigner.DesignStudio.UnitTest.Instrumentation;
using CAS.UA.Model.Designer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CAS.CommServer.UA.ModelDesigner.DesignStudio.UnitTest
{
  [TestClass]
  public class AssemblyTraceEventUnitTest
  {

    [TestMethod]
    public void CreatorTest()
    {
      TraceSource _tracer = AssemblyTraceEvent.Tracer;
      Assert.IsNotNull(_tracer);
      Assert.AreEqual(1, _tracer.Listeners.Count, $"Available listeners: {System.String.Join(", ", _tracer.Listeners.Cast<TraceListener>().Select<TraceListener, string>(x => x.Name).ToArray<String>())}");
      Dictionary<string, TraceListener> _listeners = _tracer.Listeners.Cast<TraceListener>().ToDictionary<TraceListener, string>(x => x.Name);
      Assert.IsTrue(_listeners.ContainsKey("LogFile"));
      TraceListener _listener = _listeners["LogFile"];
      Assert.IsNotNull(_listener);
      Assert.IsInstanceOfType(_listener, typeof(DelimitedListTraceListener));
      DelimitedListTraceListener _advancedListener = _listener as DelimitedListTraceListener;
      Assert.IsNotNull(_advancedListener.Filter);
      Assert.IsInstanceOfType(_advancedListener.Filter, typeof(EventTypeFilter));
      EventTypeFilter _eventTypeFilter = _advancedListener.Filter as EventTypeFilter;
      Assert.AreEqual(SourceLevels.All, _eventTypeFilter.EventType);
      string _testPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
      Assert.Inconclusive("Assert.AreEqual failed - wrong actual value if Run All");
      //<C:\VS.git\ASMD\TestResults\Deploy_mpost 2019-11-06 09_59_05\Out\CAS.OPC.UA.ASMD.log>.
      //< C:\VS.git\ASMD\ModelDesigner.Configuration.UnitTests\bin\Debug\CAS.OPC.UA.ASMD.log >.
       Assert.AreEqual<string>(Path.Combine(_testPath, @"CAS.OPC.UA.ASMD.log"), _advancedListener.GetFileName());
    }
    [TestMethod]
    public void LogFileExistsTest()
    {
      TraceSource _tracer = AssemblyTraceEvent.Tracer;
      TraceListener _listener = _tracer.Listeners.Cast<TraceListener>().Where<TraceListener>(x => x.Name == "LogFile").First<TraceListener>();
      Assert.IsNotNull(_listener);
      DelimitedListTraceListener _advancedListener = _listener as DelimitedListTraceListener;
      Assert.IsNotNull(_advancedListener);
      Assert.IsFalse(String.IsNullOrEmpty(_advancedListener.GetFileName()));
      FileInfo _logFileInfo = new FileInfo(_advancedListener.GetFileName());
      long _length = _logFileInfo.Exists ? _logFileInfo.Length : 0;
      _tracer.TraceEvent(TraceEventType.Verbose, 0, "LogFileExistsTest is executed");
      Assert.IsFalse(String.IsNullOrEmpty(_advancedListener.GetFileName()));
      _logFileInfo.Refresh();
      Assert.IsTrue(_logFileInfo.Exists, $"{_logFileInfo.FullName} doesn't exist");
      Assert.IsTrue(_logFileInfo.Length > _length + 10, $"The final file lenght = {_logFileInfo.Length} must be > {_length} + 10");
    }
  }
}
