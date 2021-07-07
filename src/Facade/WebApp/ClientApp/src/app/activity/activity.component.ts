import {ChangeDetectorRef, Component, Inject} from '@angular/core';
import { HttpClient } from '@angular/common/http';
import * as Ably from "ably";
import { Types } from "ably";
import PaginatedResult = Types.PaginatedResult;
import Message = Types.Message;

@Component({
  selector: 'app-activity',
  templateUrl: './activity.component.html'
})
export class ActivityComponent {
  public activities: Topic [];
  constructor(
    private readonly http: HttpClient,
    private readonly changeDetector: ChangeDetectorRef,
    @Inject('BASE_URL') private readonly baseUrl: string) {
    this.http.get(`${this.baseUrl}Facade`)
      .toPromise()
      .then((e: AblyConfig) => {
        let client = new Ably.Realtime({ key: e.apiKey });
        let channel = client.channels.get(e.push.name);
        channel.attach(err => {
          channel.history({untilAttach: true}, (error, results) => {
            let isMsg = (maybeMsg: PaginatedResult<Message> | undefined):
              maybeMsg is PaginatedResult<Message> =>
                (maybeMsg as PaginatedResult<Message>) !== undefined;

            if (isMsg(results))
              this.activities =
                  results
                  .items
                  .map(msg => {
                    if (msg.name === e.push.messageType) {
                      let additionalData = JSON.parse(msg.data).AdditionalData.Item;
                      return JSON.parse(additionalData);
                    }
                    else {
                      return { Creator: "Server request", Description: "Server request", Id: msg.id, Done: false };
                    }
                  });
            else
              this.activities = [];
            this.changeDetector.detectChanges();
          })})
      });
  }
}
