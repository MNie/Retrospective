import {ChangeDetectorRef, Component, Inject} from '@angular/core';
import {HttpClient} from "@angular/common/http";
import * as Ably from "ably";

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
})
export class HomeComponent {
  public topics: Topic [];

  constructor(
    private readonly http: HttpClient,
    private readonly changeDetector: ChangeDetectorRef,
    @Inject('BASE_URL') private readonly baseUrl: string) {
    this.http.get(`${this.baseUrl}Facade`)
      .toPromise()
      .then((e: AblyConfig) => {
        let client = new Ably.Realtime({ key: e.apiKey });
        let channel = client.channels.get(e.push.name);
        channel.subscribe(e.push.messageType, msg => {
            let additionalData = JSON.parse(msg.data).AdditionalData.Item;
            let newRecord = JSON.parse(additionalData);
            this.topics = [newRecord, ...this.topics];

            this.changeDetector.detectChanges();
          })
        });
    this.topics = [];
  }

  public markAsDone (model) {
    this.http.post(
      `${this.baseUrl}Facade`,
      JSON.stringify(model),
      { headers: { 'content-type': 'application/json'}})
      .toPromise()
      .then(_ => {
        this.topics = this.topics.filter(x => x !== model);
        this.changeDetector.detectChanges();
      });
  }
}

