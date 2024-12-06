# WorkflowController

Controls workflow progression and step validation

Inherits from: `MonoBehaviour`

## Properties

### destroyCancellationToken
- Type: `CancellationToken`
- Access: Public

### useGUILayout
- Type: `Boolean`
- Access: Public

### didStart
- Type: `Boolean`
- Access: Public

### didAwake
- Type: `Boolean`
- Access: Public

### runInEditMode
- Type: `Boolean`
- Access: Public

### enabled
- Type: `Boolean`
- Access: Public

### isActiveAndEnabled
- Type: `Boolean`
- Access: Public

### transform
- Type: `Transform`
- Access: Public

### gameObject
- Type: `GameObject`
- Access: Public

### tag
- Type: `String`
- Access: Public

### rigidbody
- Type: `Component`
- Access: Public

### rigidbody2D
- Type: `Component`
- Access: Public

### camera
- Type: `Component`
- Access: Public

### light
- Type: `Component`
- Access: Public

### animation
- Type: `Component`
- Access: Public

### constantForce
- Type: `Component`
- Access: Public

### renderer
- Type: `Component`
- Access: Public

### audio
- Type: `Component`
- Access: Public

### networkView
- Type: `Component`
- Access: Public

### collider
- Type: `Component`
- Access: Public

### collider2D
- Type: `Component`
- Access: Public

### hingeJoint
- Type: `Component`
- Access: Public

### particleSystem
- Type: `Component`
- Access: Public

### name
- Type: `String`
- Access: Public

### hideFlags
- Type: `HideFlags`
- Access: Public

## Methods

### LogCurrentStepInfo
```csharp
public Void LogCurrentStepInfo()
```

### ResetWorkflow
```csharp
public Void ResetWorkflow()
```

### DebugButtonStates
```csharp
public Void DebugButtonStates()
```

### SaveProgress
```csharp
public Void SaveProgress()
```

### LoadProgress
```csharp
public Void LoadProgress()
```

### IsInvoking
```csharp
public Boolean IsInvoking()
```

### CancelInvoke
```csharp
public Void CancelInvoke()
```

### Invoke
```csharp
public Void Invoke(String methodName, Single time)
```

### InvokeRepeating
```csharp
public Void InvokeRepeating(String methodName, Single time, Single repeatRate)
```

### CancelInvoke
```csharp
public Void CancelInvoke(String methodName)
```

### IsInvoking
```csharp
public Boolean IsInvoking(String methodName)
```

### StartCoroutine
```csharp
public Coroutine StartCoroutine(String methodName)
```

### StartCoroutine
```csharp
public Coroutine StartCoroutine(String methodName, Object value)
```

### StartCoroutine
```csharp
public Coroutine StartCoroutine(IEnumerator routine)
```

### StartCoroutine_Auto
```csharp
public Coroutine StartCoroutine_Auto(IEnumerator routine)
```

### StopCoroutine
```csharp
public Void StopCoroutine(IEnumerator routine)
```

### StopCoroutine
```csharp
public Void StopCoroutine(Coroutine routine)
```

### StopCoroutine
```csharp
public Void StopCoroutine(String methodName)
```

### StopAllCoroutines
```csharp
public Void StopAllCoroutines()
```

### GetComponent
```csharp
public Component GetComponent(Type type)
```

### GetComponent
```csharp
public T GetComponent()
```

### TryGetComponent
```csharp
public Boolean TryGetComponent(Type type, Component& component)
```

### TryGetComponent
```csharp
public Boolean TryGetComponent(T& component)
```

### GetComponent
```csharp
public Component GetComponent(String type)
```

### GetComponentInChildren
```csharp
public Component GetComponentInChildren(Type t, Boolean includeInactive)
```

### GetComponentInChildren
```csharp
public Component GetComponentInChildren(Type t)
```

### GetComponentInChildren
```csharp
public T GetComponentInChildren(Boolean includeInactive)
```

### GetComponentInChildren
```csharp
public T GetComponentInChildren()
```

### GetComponentsInChildren
```csharp
public Component[] GetComponentsInChildren(Type t, Boolean includeInactive)
```

### GetComponentsInChildren
```csharp
public Component[] GetComponentsInChildren(Type t)
```

### GetComponentsInChildren
```csharp
public T[] GetComponentsInChildren(Boolean includeInactive)
```

### GetComponentsInChildren
```csharp
public Void GetComponentsInChildren(Boolean includeInactive, List`1 result)
```

### GetComponentsInChildren
```csharp
public T[] GetComponentsInChildren()
```

### GetComponentsInChildren
```csharp
public Void GetComponentsInChildren(List`1 results)
```

### GetComponentInParent
```csharp
public Component GetComponentInParent(Type t, Boolean includeInactive)
```

### GetComponentInParent
```csharp
public Component GetComponentInParent(Type t)
```

### GetComponentInParent
```csharp
public T GetComponentInParent(Boolean includeInactive)
```

### GetComponentInParent
```csharp
public T GetComponentInParent()
```

### GetComponentsInParent
```csharp
public Component[] GetComponentsInParent(Type t, Boolean includeInactive)
```

### GetComponentsInParent
```csharp
public Component[] GetComponentsInParent(Type t)
```

### GetComponentsInParent
```csharp
public T[] GetComponentsInParent(Boolean includeInactive)
```

### GetComponentsInParent
```csharp
public Void GetComponentsInParent(Boolean includeInactive, List`1 results)
```

### GetComponentsInParent
```csharp
public T[] GetComponentsInParent()
```

### GetComponents
```csharp
public Component[] GetComponents(Type type)
```

### GetComponents
```csharp
public Void GetComponents(Type type, List`1 results)
```

### GetComponents
```csharp
public Void GetComponents(List`1 results)
```

### GetComponents
```csharp
public T[] GetComponents()
```

### GetComponentIndex
```csharp
public Int32 GetComponentIndex()
```

### CompareTag
```csharp
public Boolean CompareTag(String tag)
```

### CompareTag
```csharp
public Boolean CompareTag(TagHandle tag)
```

### SendMessageUpwards
```csharp
public Void SendMessageUpwards(String methodName, Object value, SendMessageOptions options)
```

### SendMessageUpwards
```csharp
public Void SendMessageUpwards(String methodName, Object value)
```

### SendMessageUpwards
```csharp
public Void SendMessageUpwards(String methodName)
```

### SendMessageUpwards
```csharp
public Void SendMessageUpwards(String methodName, SendMessageOptions options)
```

### SendMessage
```csharp
public Void SendMessage(String methodName, Object value)
```

### SendMessage
```csharp
public Void SendMessage(String methodName)
```

### SendMessage
```csharp
public Void SendMessage(String methodName, Object value, SendMessageOptions options)
```

### SendMessage
```csharp
public Void SendMessage(String methodName, SendMessageOptions options)
```

### BroadcastMessage
```csharp
public Void BroadcastMessage(String methodName, Object parameter, SendMessageOptions options)
```

### BroadcastMessage
```csharp
public Void BroadcastMessage(String methodName, Object parameter)
```

### BroadcastMessage
```csharp
public Void BroadcastMessage(String methodName)
```

### BroadcastMessage
```csharp
public Void BroadcastMessage(String methodName, SendMessageOptions options)
```

### GetInstanceID
```csharp
public Int32 GetInstanceID()
```

### GetHashCode
```csharp
public Int32 GetHashCode()
```

### Equals
```csharp
public Boolean Equals(Object other)
```

### ToString
```csharp
public String ToString()
```

### GetType
```csharp
public Type GetType()
```

