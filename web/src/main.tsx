import {StrictMode} from "react";
import {createRoot} from "react-dom/client";
import App from "./App";
import "./index.css";
import {DraftProvider} from "./context/DraftProvider.tsx";
import {ErrorBoundary} from "./components/errorHandling/ErrorBoundary.tsx";

const container = document.getElementById("root")!;

const root = createRoot(container);

root.render(
    <StrictMode>
      <ErrorBoundary>
        <DraftProvider>
          <App />
        </DraftProvider>
      </ErrorBoundary>
    </StrictMode>
)
