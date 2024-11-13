import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ApiConfigService {
  readonly DB_APIUrl = "https://localhost:7204/api/DBController/";
  readonly OPC_APIUrl = "https://localhost:7204/api/OPCController/";
}
