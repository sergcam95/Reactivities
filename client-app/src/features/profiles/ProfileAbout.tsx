import React, { useContext, useState, useEffect } from "react";
import { Tab, Grid, Header, Button, Form } from "semantic-ui-react";
import { RootStoreContext } from "../../app/stores/rootStore";
import { Form as FinalForm, Field } from "react-final-form";
import TextInput from "../../app/common/form/TextInput";
import TextAreaInput from "../../app/common/form/TextAreaInput";
import { combineValidators, isRequired } from "revalidate";
import { observer } from "mobx-react-lite";
import { ProfileFormValues } from "../../app/models/profile";

const validate = combineValidators({
  displayName: isRequired({ message: "The display name is required" }),
});

const ProfileAbout: React.FC = () => {
  const rootStore = useContext(RootStoreContext);
  const { isCurrentUser, submitting, updateProfile } = rootStore.profileStore;

  const [editMode, setEditMode] = useState(false);
  const [loading, setLoading] = useState(false);
  const [profile, setProfile] = useState(new ProfileFormValues());

  const handleFinalFormSubmit = (values: any) => {
    const { ...profile } = values;
    updateProfile(profile).then(() => setEditMode(false));
  };

  useEffect(() => {
    setLoading(true);
    setProfile(new ProfileFormValues(rootStore.profileStore.profile!));
    setLoading(false);
  }, [rootStore.profileStore.profile]);

  return (
    <Tab.Pane>
      <Grid>
        <Grid.Column width={16}>
          <Header
            floated="left"
            icon="user"
            content={`About ${profile.displayName}`}
          />
          {isCurrentUser && (
            <Button
              floated="right"
              basic
              type="button"
              content={editMode ? "Cancel" : "Edit Profile"}
              onClick={() => setEditMode(!editMode)}
            />
          )}
        </Grid.Column>
        {!editMode && (
          <Grid.Column width={16}>
            <span>{profile!.bio}</span>
          </Grid.Column>
        )}
        {editMode && (
          <Grid.Column width={16}>
            <FinalForm
              initialValues={profile}
              validate={validate}
              onSubmit={handleFinalFormSubmit}
              render={({ handleSubmit, invalid, pristine }) => (
                <Form onSubmit={handleSubmit} loading={loading} error>
                  <Field
                    name="displayName"
                    component={TextInput}
                    placeholder="DisplayName"
                    value={profile.displayName}
                  />
                  <Field
                    name="bio"
                    placeholder="Bio"
                    rows={3}
                    value={profile.bio}
                    component={TextAreaInput}
                  />
                  <Button
                    positive
                    floated="right"
                    type="submit"
                    loading={submitting}
                    content="Update profile"
                    disabled={loading || invalid || pristine}
                  />
                </Form>
              )}
            />
          </Grid.Column>
        )}
      </Grid>
    </Tab.Pane>
  );
};

export default observer(ProfileAbout);
