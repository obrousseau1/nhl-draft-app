import {Component, type ErrorInfo, type ReactNode} from "react";

type Props = { children: ReactNode }
type State = { hasError: boolean }

export class ErrorBoundary extends Component<Props, State> {
  state: State = { hasError: false }

  static getDerivedStateFromError(): State {
    return { hasError: true }
  }

  componentDidCatch(error: Error, info: ErrorInfo) {
    console.error('Crash while drawing:', error, info.componentStack)
  }

  render() {
    if (this.state.hasError)
      return (
        <div>
          <p>Something went wrong. Your picks are saved on the server. Reload to continue.</p>
          <button onClick={()=>window.location.reload()}>Reload</button>
        </div>
      )
    return this.props.children
  }
}
