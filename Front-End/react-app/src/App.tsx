import { useState } from "react";
import Alert from "./Components/Alert";
import Button from "./Components/Button";
import ListGroup from "./Components/ListGroup";

function App() {
  let items = ["New York", "London", "Paris", "Tokyo", "Kyiv"];

  const handleSelectedItem = (item: string) => {
    console.log(item);
  };

  const [alertVisible, setAlertVisible] = useState(false);

  return (
    <>
      <ListGroup
        items={items}
        heading="List"
        onSelectItem={handleSelectedItem}
      />
      {alertVisible && (
        <Alert onClose={() => setAlertVisible(false)}>
          Hello <span>Alert</span>
        </Alert>
      )}
      <Button onClickButton={() => setAlertVisible(true)} color="primary">
        Press it
      </Button>
    </>
  );
}

export default App;
