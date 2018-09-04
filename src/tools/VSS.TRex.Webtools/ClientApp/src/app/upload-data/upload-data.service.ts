import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { HttpHeaders } from '@angular/common/http';


import { Observable } from 'rxjs';
import { catchError } from 'rxjs/operators';

import { ITagFileRequest } from './tagfile-request';
import { HttpErrorHandler, HandleError } from '../http-error-handler.service';

const httpOptions = {
  headers: new HttpHeaders({
    'Content-Type': 'application/json',
    'x-visionlink-customeruid': '00000',
    'authorization': 'dont care'
  })
};

@Injectable()
export class UploadDataService {
  tagUploadUrl = 'http://trex.mutable.dev.k8s.vspengg.com/api/v2/tagfiles';  // URL to web api
  private handleError: HandleError;

  constructor(
    private http: HttpClient,
    httpErrorHandler: HttpErrorHandler) {
    this.handleError = httpErrorHandler.createHandleError('UploadDataService');
  }

  ///** GET heroes from the server */
  //getHeroes(): Observable<Hero[]> {
  //  return this.http.get<Hero[]>(this.heroesUrl)
  //    .pipe(
  //      catchError(this.handleError('getHeroes', []))
  //    );
  //}

  /* GET heroes whose name contains search term */
  //searchHeroes(term: string): Observable<Hero[]> {
  //  term = term.trim();

  //  // Add safe, URL encoded search parameter if there is a search term
  //  const options = term ?
  //    { params: new HttpParams().set('name', term) } : {};

  //  return this.http.get<Hero[]>(this.heroesUrl, options)
  //    .pipe(
  //      catchError(this.handleError<Hero[]>('searchHeroes', []))
  //    );
  //}

  //////// Save methods //////////

  /** POST: submit a tag file to trex */
  addTagfile(tagFile: ITagFileRequest): Observable<ITagFileRequest> {
    return this.http.post<ITagFileRequest>(this.tagUploadUrl, tagFile, httpOptions)
      .pipe(
        catchError(this.handleError('submitTagFile', tagFile))
      );
  }

  ///** DELETE: delete the hero from the server */
  //deleteHero(id: number): Observable<{}> {
  //  const url = `${this.heroesUrl}/${id}`; // DELETE api/heroes/42
  //  return this.http.delete(url, httpOptions)
  //    .pipe(
  //      catchError(this.handleError('deleteHero'))
  //    );
  //}

  ///** PUT: update the hero on the server. Returns the updated hero upon success. */
  //updateHero(hero: Hero): Observable<Hero> {
  //  httpOptions.headers =
  //    httpOptions.headers.set('Authorization', 'my-new-auth-token');

  //  return this.http.put<Hero>(this.heroesUrl, hero, httpOptions)
  //    .pipe(
  //      catchError(this.handleError('updateHero', hero))
  //    );
  //}
}

