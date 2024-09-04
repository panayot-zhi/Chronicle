/*---------------------------------------------------------------------------------------------
 *  **DO NOT EDIT** - This file is an automatically generated file.
 *--------------------------------------------------------------------------------------------*/

/* eslint-disable sort-imports */
/* eslint-disable @typescript-eslint/no-empty-interface */
// eslint-disable-next-line header/header
import { Command, CommandPropertyValidators, CommandValidator } from '@cratis/applications/commands';
import { useCommand, SetCommandValues, ClearCommandValues } from '@cratis/applications.react/commands';
import { Validator } from '@cratis/applications/validation';
import { Causation } from '../Auditing/Causation';
import { Identity } from '../Identities/Identity';
import { EventToAppend } from './EventToAppend';
import Handlebars from 'handlebars';

const routeTemplate = Handlebars.compile('/api/events/store/{{eventStore}}/{{namespace}}/sequence/{{eventSequenceId}}/append-many');

export interface IAppendMany {
    eventStore?: string;
    namespace?: string;
    eventSequenceId?: string;
    eventSourceId?: string;
    events?: EventToAppend[];
    causation?: Causation[];
    causedBy?: Identity;
}

export class AppendManyValidator extends CommandValidator {
    readonly properties: CommandPropertyValidators = {
        eventStore: new Validator(),
        namespace: new Validator(),
        eventSequenceId: new Validator(),
        eventSourceId: new Validator(),
        events: new Validator(),
        causation: new Validator(),
        causedBy: new Validator(),
    };
}

export class AppendMany extends Command<IAppendMany> implements IAppendMany {
    readonly route: string = '/api/events/store/{eventStore}/{namespace}/sequence/{eventSequenceId}/append-many';
    readonly routeTemplate: Handlebars.TemplateDelegate = routeTemplate;
    readonly validation: CommandValidator = new AppendManyValidator();

    private _eventStore!: string;
    private _namespace!: string;
    private _eventSequenceId!: string;
    private _eventSourceId!: string;
    private _events!: EventToAppend[];
    private _causation!: Causation[];
    private _causedBy!: Identity;

    constructor() {
        super(Object, false);
    }

    get requestArguments(): string[] {
        return [
            'eventStore',
            'namespace',
            'eventSequenceId',
        ];
    }

    get properties(): string[] {
        return [
            'eventStore',
            'namespace',
            'eventSequenceId',
            'eventSourceId',
            'events',
            'causation',
            'causedBy',
        ];
    }

    get eventStore(): string {
        return this._eventStore;
    }

    set eventStore(value: string) {
        this._eventStore = value;
        this.propertyChanged('eventStore');
    }
    get namespace(): string {
        return this._namespace;
    }

    set namespace(value: string) {
        this._namespace = value;
        this.propertyChanged('namespace');
    }
    get eventSequenceId(): string {
        return this._eventSequenceId;
    }

    set eventSequenceId(value: string) {
        this._eventSequenceId = value;
        this.propertyChanged('eventSequenceId');
    }
    get eventSourceId(): string {
        return this._eventSourceId;
    }

    set eventSourceId(value: string) {
        this._eventSourceId = value;
        this.propertyChanged('eventSourceId');
    }
    get events(): EventToAppend[] {
        return this._events;
    }

    set events(value: EventToAppend[]) {
        this._events = value;
        this.propertyChanged('events');
    }
    get causation(): Causation[] {
        return this._causation;
    }

    set causation(value: Causation[]) {
        this._causation = value;
        this.propertyChanged('causation');
    }
    get causedBy(): Identity {
        return this._causedBy;
    }

    set causedBy(value: Identity) {
        this._causedBy = value;
        this.propertyChanged('causedBy');
    }

    static use(initialValues?: IAppendMany): [AppendMany, SetCommandValues<IAppendMany>, ClearCommandValues] {
        return useCommand<AppendMany, IAppendMany>(AppendMany, initialValues);
    }
}
