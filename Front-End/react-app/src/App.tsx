import ListGroup from "./Components/ListGroup";

function App() {
  let items = ["New York", "London", "Paris", "Tokyo", "Kyiv"];
  return (
    <div>
      <ListGroup items={items} heading="List" />
    </div>
  );
}

export default App;
