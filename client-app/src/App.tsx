import React, { useState, useEffect } from "react";
import axios from "axios";

import { Header, Icon, List } from "semantic-ui-react";
import "./App.css";

interface IValues {
  id: number;
  name: string;
}

const App = () => {
  const [values, setValues] = useState<IValues[]>([]);

  useEffect(() => {
    axios.get("http://localhost:5000/api/values").then((response) => {
      setValues(response.data);
    });
  }, []);

  return (
    <Header as="h2">
      <Icon name="plug" />
      <Header.Content>Reactivities</Header.Content>
      <List>
        {values.map((value) => (
          <List.Item key={value.id}>{value.name}</List.Item>
        ))}
      </List>
    </Header>
  );
};

export default App;
