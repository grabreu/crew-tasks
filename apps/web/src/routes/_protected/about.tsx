import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/_protected/about")({
  component: RouteComponent,
});

function RouteComponent() {
  return <div>Hello "/_protected/about"!</div>;
}
