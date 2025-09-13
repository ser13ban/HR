export interface FeedbackListDto {
  id: number;
  fromEmployeeName: string;
  content: string;
  polishedContent?: string;
  type: FeedbackType;
  rating: number;
  isAnonymous: boolean;
  isPolished: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateFeedbackDto {
  toEmployeeId: number;
  content: string;
  type: FeedbackType;
  rating: number;
  isAnonymous: boolean;
}

export interface FeedbackDetailDto {
  id: number;
  fromEmployeeId: number;
  fromEmployeeName: string;
  toEmployeeId: number;
  toEmployeeName: string;
  content: string;
  polishedContent?: string;
  type: FeedbackType;
  rating: number;
  isAnonymous: boolean;
  isPolished: boolean;
  createdAt: string;
  updatedAt: string;
}

export enum FeedbackType {
  General = 0,
  Performance = 1,
  Collaboration = 2,
  Communication = 3,
  Leadership = 4,
  Technical = 5
}

export const FeedbackTypeLabels: { [key in FeedbackType]: string } = {
  [FeedbackType.General]: 'General',
  [FeedbackType.Performance]: 'Performance',
  [FeedbackType.Collaboration]: 'Collaboration',
  [FeedbackType.Communication]: 'Communication',
  [FeedbackType.Leadership]: 'Leadership',
  [FeedbackType.Technical]: 'Technical'
};
