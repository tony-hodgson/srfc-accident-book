export interface Accident {
  id?: number;
  dateOfAccident: string;
  timeOfAccident: string;
  location: string;
  opposition: string;
  personInvolved: string;
  age?: number;
  personReporting: string;
  description: string;
  natureOfInjury: string;
  treatmentGiven: string;
  actionTaken: string;
  witnesses: string;
  createdAt?: string;
  updatedAt?: string;
}

