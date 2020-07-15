import React from "react";
import { Dimmer, Loader } from "semantic-ui-react";

const LoadingComponent: React.FC<{
  inverted?: boolean;
  content?: string;
}> = (props) => {
  return (
    <Dimmer active inverted={props.inverted ?? true}>
      <Loader content={props.content} />
    </Dimmer>
  );
};

export default LoadingComponent;
