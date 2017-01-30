h1. CodeProbe.ExecutionControl

The library is based on the existence o an _execution context_ that can have a key-value pair collection associated, which can be examined and modified in every step of the calls chain originated inseide the context, be them locals on different threads or remote.

The current context, if exists, can be obtained from the static property of the object *ExecutionControlManager*:

	IExecutionContext ctx = ExecutionControlManager.Current;


The object *ExecutionControlManager* is the entrypoint for the majority of the library functionsand in particular exposes all the methods for *create* a context, *enter* a context if exists, *obtain* a context (get or create).

	ExecutionControlManager.CreateScope();
	ExecutionControlManager.EnterScope();
	ExecutionControlManager.GetScope();


Every methods returns an _IDisposable_, hence can be used eith the _using_ construct. The objects are built in such a way that the context is kept until the end of the external block (the one that called directly or indirectly the method _CreateScope_).


	using (ExecutionControlManager.GetScope())
	{
	      IExecutionContext ctx=ExecutionControlManager.Current;
	      ctx.SetCtxValue("name", expected);
	      ctx.GetCtxValue<int>("name")
	}

In the example arepresent the two main method of a context:


- *SetCtxValue*: sets a value in a context
- *GetCtxValue<T>*: get and cast a value from a context

**NOTE:**
The context value must be serializable objects beacuse they have to travel thorough the net.

By default the context doesn't travel to remote WCF services. For the propagation exists two modes: explicit and implicit.

### Explicit mode

To propagate the context in explicit mode must be used the method *SpanRemote*:

	using (ChannelFactory<ITestService> fc = new ChannelFactory<ITestService>(new BasicHttpBinding()))
	{
	      using (ExecutionControlManager.GetScope())
	      {
	            IExecutionContext ctx = ExecutionControlManager.Current.SpanRemote<ITestService>(fc);

### Implicit mode

To enable the implicit context propagtion it is necessary to modify the endpoint bahviour of the service using the  *ExecutionContextSpanBehavior*
	
	<system.serviceModel>
	  <extensions>
	    <behaviorExtensions>
	      <add name="ExecutionContextSpan" type="CodeProbe.ExecutionControl.ExecutionContextSpanBehaviorExtensionElement, CodeProbe.ExecutionControl"/>
	    </behaviorExtensions>
	  </extensions>
	  <behaviors>
	    <endpointBehaviors>
	      <behavior name="TestSpan">
	        <ExecutionContextSpan />
	      </behavior>
	    </endpointBehaviors>
	  </behaviors>
	  <services>
	    <service name="TestService">
	      <endpoint behaviorConfiguration="TestSpan" binding="basicHttpBinding" address="http://localhost:12345/testService" contract="Test.CodeProbe.ExecutionControl.RemoteUsage.ITestService" />
	    </service>
	  </services>
	</system.serviceModel>