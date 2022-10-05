
namespace GPU_Of_Life;

/// <summary>
/// Represents a history of a stream of values.
/// </summary>
public abstract class History<TValue, TAggregation>
{
    protected internal class Record
    {
        internal bool Is__State__Undo { get; set; }
        internal bool Is__State__Dead_Undo { get; set; }

        internal TValue Value { get; set; }

        public Record(TValue value)
        {
            Value = value;
        }
    }

    protected internal class Epoch
    {
        public Texture EPOCH__TEXTURE;

        public readonly Record[] EPOCH__HISTORY;
        public int EPOCH__HISTORY_INDEX { get; internal set; }
        public bool Is__Epoch_Finished
            => EPOCH__HISTORY_INDEX == EPOCH__HISTORY.Length || Is__Epoch_Empty;
        public bool Is__Epoch_Empty
            => EPOCH__HISTORY_INDEX <= 0;
        public bool Is__Epoch_Undone
            => EPOCH__HISTORY[0]?.Is__State__Undo ?? true;

        public IEnumerable<TValue> Records
        {
            get 
            {
                for(int i=0;i<EPOCH__HISTORY_INDEX;i++)
                {
                    if (EPOCH__HISTORY[EPOCH__HISTORY_INDEX]?.Is__State__Undo ?? true) 
                        continue;
                    yield return EPOCH__HISTORY[EPOCH__HISTORY_INDEX].Value;
                }
            }
        }

        public Epoch(int epoch_size, int texture_width, int texture_height)
        {
            EPOCH__HISTORY = new Record[epoch_size];
            EPOCH__TEXTURE =
                new Texture(texture_width, texture_height);
        }

        public void Resize(int width, int height, ref bool error)
            => EPOCH__TEXTURE.Resize__Texture(width, height, ref error);

        public bool Append(Record record)
        {
            if (Is__Epoch_Finished) return false;

            EPOCH__HISTORY[++EPOCH__HISTORY_INDEX] = record;
            if (!Is__Epoch_Finished && EPOCH__HISTORY[EPOCH__HISTORY_INDEX + 1].Is__State__Undo)
                EPOCH__HISTORY[EPOCH__HISTORY_INDEX + 1].Is__State__Dead_Undo = true;

            return true;
        }

        public bool Undo()
        {
            if (Is__Epoch_Empty) return false;

            EPOCH__HISTORY[--EPOCH__HISTORY_INDEX].Is__State__Undo = true;

            return true;
        }

        public bool Redo()
        {
            if (Is__Epoch_Finished) return false;
            if (EPOCH__HISTORY[EPOCH__HISTORY_INDEX] == null) return false;
            if (EPOCH__HISTORY[EPOCH__HISTORY_INDEX + 1].Is__State__Dead_Undo) return false;

            EPOCH__HISTORY[EPOCH__HISTORY_INDEX++].Is__State__Undo = false;

            return true;
        }
    }

    /// <summary>
    /// This is a record currently being developed by the user.
    /// We want to treat a drawn line, or anything else as not
    /// final until the user lets go of the mouse button.
    ///
    /// So this specific record will be rendered on an
    /// overlay, to show the user what their input will look like
    /// prior to release of the mouse button.
    /// </summary>
    private Record? Preparing__Record;

    private readonly int EPOCH__SIZE;
    private int GRID__WIDTH;
    private int GRID__HEIGHT;

    private readonly Epoch[] EPOCHS;
    private int EPOCH__INDEX = -1;

    private int Index__Next_Epoch
        => 
        (EPOCH__INDEX + 1) 
        % 
        EPOCHS.Length
        ;
    private int Index__Previous_Epoch
        => 
        (
            (EPOCH__INDEX + 1)
            %
            EPOCHS.Length
            +
            EPOCHS.Length
        )
        %
        EPOCHS.Length
        ;

    private Epoch Currently_Indexed__Epoch
        => EPOCHS[EPOCH__INDEX];
    private bool Is__Previous_Epoch_Existant
        => EPOCHS[Index__Previous_Epoch] != null;

    protected Epoch Active__Epoch
    {
        get 
        {
            if (Currently_Indexed__Epoch.Is__Epoch_Undone && Is__Previous_Epoch_Existant)
            {
                Private_Move__Epoch_Index(is__progressing_forwards_or_backwards: false);
                return Currently_Indexed__Epoch;
            }
            if (Currently_Indexed__Epoch.Is__Epoch_Finished)
                Private_Move__Epoch_Index
                (
                    is__progressing_forwards_or_backwards: true, 
                    is__creating_new_epoch: true
                );

            return Currently_Indexed__Epoch;
        }
    }

    public History
    (
        int history_epoch_size, 
        int history_epoch_count
    )
    {
        EPOCH__SIZE = history_epoch_size;
        EPOCHS =
            new Epoch[history_epoch_count];
    }

    public virtual void Resize
    (
        int width, int height, 
        ref bool error
    )
    {
        //TODO: Corner case.
        //      what if for length N epoch array
        //      from 1->M st M < N, an epoch fails
        //      to resize? Issue: mismatching resize
        //      among epochs.
        for(int i=0;i<EPOCHS.Length && !error;i++)
            EPOCHS[i]?.Resize(width, height, ref error);
        if (!error)
        {
            GRID__WIDTH = width;
            GRID__HEIGHT = height;
        }
    }

    /// <summary>
    /// Records the given value as a preparing value.
    /// Once Finish() is invoked, the preparing value will be
    /// moved onto the latest history epoch.
    ///
    /// Give null to remove the current preparing value.
    /// </summary>
    public virtual void Prepare(TValue? hot_value)
    {
        Preparing__Record = 
            (hot_value != null)
            ? new Record(hot_value)
            : null
            ;
    }

    /// <summary>
    /// Moves the preparing value from Prepare(hot_value)
    /// to the latest history epoch. If there is no value being
    /// prepared, nothing happens.
    /// </summary>
    public virtual void Finish()
    {
        if (Preparing__Record == null) return;

        Active__Epoch.Append(Preparing__Record);
        Preparing__Record = null;
    }

    public virtual void Undo()
    {
        Active__Epoch.Undo();
    }

    public virtual void Redo()
    {
        Active__Epoch.Redo();
    }

    /// <summary>
    /// Compiles all epochs down to a single
    /// aggregate value. The resulting value
    /// as a result of the history of records.
    /// </summary>
    public abstract TAggregation Aggregate__Epochs();

    private void Private_Move__Epoch_Index
    (
        bool is__progressing_forwards_or_backwards,
        bool is__creating_new_epoch = false
    )
    {
        EPOCH__INDEX =
            (is__progressing_forwards_or_backwards)
            ? Index__Next_Epoch
            : Index__Previous_Epoch
            ;

        if (is__creating_new_epoch)
            EPOCHS[EPOCH__INDEX] =
                new Epoch(EPOCH__SIZE, GRID__WIDTH, GRID__HEIGHT);
    }
}
