export interface IProfile {
  displayName: string;
  username: string;
  bio: string;
  image: string;
  following: boolean;
  followersCount: number;
  followingCount: number;
  photos: IPhoto[];
}

export interface IPhoto {
  id: string;
  url: string;
  isMain: boolean;
}

export interface IProfileFormValues extends Partial<IProfile> {}

export class ProfileFormValues implements IProfileFormValues {
  displayName: string = "";
  bio: string = "";

  constructor(init?: IProfileFormValues) {
    Object.assign(this, init);
  }
}

export interface IUserActivity {
  id: string;
  title: string;
  category: string;
  date: Date;
}
