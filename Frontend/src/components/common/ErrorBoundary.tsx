import { Component, type ErrorInfo, type ReactNode } from "react";

interface Props {
  children: ReactNode;
  fallback?: ReactNode; // Optional custom fallback UI
}

interface State {
  hasError: boolean;
}



/**
 * A general and customizeable error boundary for the website
 *
 * @class ErrorBoundary
 * @extends {Component<Props, State>}
 */
class ErrorBoundary extends Component<Props, State> {
  state: State = { hasError: false };

  static getDerivedStateFromError(): State {
    return { hasError: true }; // Update state to trigger fallback UI
  }

  componentDidCatch(error: Error, info: ErrorInfo) {
    console.error("Caught by ErrorBoundary:", error, info);
    // Optionally send error to external service here
    // logErrorToMonitoringService(error, info);
  }

  render() {
    if (this.state.hasError) {
      return this.props.fallback ?? (
        <div style={{ padding: "1rem", backgroundColor: "#ffeeee", color: "#cc0000" }}>
          <h2>Something went wrong.</h2>
          <p>Please refresh the page or contact support.</p>
        </div>
      );
    }

    return this.props.children;
  }
}

export default ErrorBoundary;