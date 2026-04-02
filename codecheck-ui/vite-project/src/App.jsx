import { useState } from "react";
import Editor from "@monaco-editor/react";

function App() {
  const [code, setCode] = useState("public void Test(){int x=1;}");
  const [result, setResult] = useState(null);
  const [loading, setLoading] = useState(false);

  const analyze = async () => {
    const res = await fetch("https://localhost:7148/api/Analyze", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        code: code,
        language: "csharp",
        mode: "optimize",
      }),
    });

    const data = await res.json();
    setResult(data);
  };

  return (
    <div style={{ padding: 20 }}>
      <h1>AI Code Assistant</h1>

      <Editor
        height="300px"
        defaultLanguage="csharp"
        value={code}
        onChange={(value) => setCode(value)}
      />
      <div style={{textAlign: "center", marginTop: 15}}>
        <button onClick={()=>analyze("explain")}>Analyze</button>
        <button onClick={()=>analyze("bugs")}>Bugs</button>
        <button onClick={()=>analyze("optimize")}>optimize</button>
        <button onClick={()=>analyze("fix")}>Fix</button>

      </div>

        {loading &&(
          <p style={{textAlign: "center", marginTop: 10}}>
            Ai thinking...
          </p>
        )}
     
            {result && (
        <div style={{ marginTop: 20 }}>
          <h2>Summary</h2>
          <p>{result.summary}</p>

          <h3>Issues</h3>
          <ul>
            {result.issues?.map((i, idx) => (
              <li key={idx}>{i}</li>
            ))}
          </ul>

          <h3>Improvements</h3>
          <ul>
            {result.improvements?.map((i, idx) => (
              <li key={idx}>{i}</li>
            ))}
          </ul>

          <h3>Refactored Code</h3>
          <pre>{result.refactoredCode}</pre>
        </div>
      )}
    </div>
  );
}

export default App;