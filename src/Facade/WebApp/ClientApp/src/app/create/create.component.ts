import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-create-task',
  templateUrl: './create.component.html'
})
export class CreateComponent {
  public model: Create = { Description: "", Creator: "" };
  constructor(
    private readonly http: HttpClient,
    @Inject('BASE_URL') private readonly baseUrl: string) {
  }

  public onSubmit = () =>
    this.http.post(
        `${this.baseUrl}Facade`,
        JSON.stringify(this.model),
      { headers: { 'content-type': 'application/json'}})
      .toPromise()
      .then(_ => this.reset());

  public reset = () =>
    this.model = { Description: "", Creator: "" }
}
