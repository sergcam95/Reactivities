import React, { useContext } from "react";
import { observer } from "mobx-react-lite";

import { Item, Button, Label, Segment } from "semantic-ui-react";

import ActivityStore from "../../../app/stores/activityStore";

const ActivityList: React.FC = (props) => {
  const activityStore = useContext(ActivityStore);
  const {
    activitiesByDate,
    selectActivity,
    submitting,
    deleteActivity,
    target,
  } = activityStore;

  return (
    <Segment clearing>
      <Item.Group divided>
        {activitiesByDate.map((activity) => (
          <Item key={activity.id}>
            <Item.Content>
              <Item.Header as="a">{activity.title}</Item.Header>
              <Item.Meta>{activity.date}</Item.Meta>
              <Item.Description>
                <div>{activity.description}</div>
                <div>
                  {activity.city}, {activity.venue}
                </div>
              </Item.Description>
              <Item.Extra>
                <Button
                  onClick={() => selectActivity(activity.id)}
                  floated="right"
                  content="View"
                  color="blue"
                />
                <Button
                  name={activity.id}
                  onClick={(e) => deleteActivity(e, activity.id)}
                  floated="right"
                  content="Delete"
                  color="red"
                  loading={target === activity.id && submitting}
                />
                <Label basic content={activity.category} />
              </Item.Extra>
            </Item.Content>
          </Item>
        ))}
      </Item.Group>
    </Segment>
  );
};

export default observer(ActivityList);