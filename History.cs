
namespace GPU_Of_Life;

/// <summary>
/// Represents a history of a stream of values.
/// </summary>
public abstract class History<TRecord_Value, TAggregation>
{
    protected internal class Record
    {
        internal bool Is__State__Undo { get; set; }
        internal bool Is__State__Dead_Undo { get; set; }
    }

    protected internal class Epoch
    {
        public bool Is__In_Need_Of__Update { get; internal set; }
            = true;

        public readonly TRecord_Value[] EPOCH__VALUES;
        internal readonly Record[] EPOCH__RECORDS;
        public int EPOCH__HISTORY_INDEX { get; internal set; }
        public bool Is__Epoch_Finished
            => EPOCH__HISTORY_INDEX >= EPOCH__RECORDS.Length;
        public bool Is__Epoch_Empty
            => EPOCH__HISTORY_INDEX <= 0;
        public bool Is__Epoch_Undone
            => EPOCH__RECORDS[0]?.Is__State__Undo ?? false;

        public IEnumerable<TRecord_Value> Active__Values
        {
            get 
            {
                for(int i=0;i<EPOCH__HISTORY_INDEX;i++)
                {
                    if (EPOCH__RECORDS[i] == null) yield break;
                    if (EPOCH__RECORDS[i]?.Is__State__Undo ?? true) 
                        continue;
                    yield return EPOCH__VALUES[i];
                }
            }
        }

        public IEnumerable<Record> Active__Records
        {
            get 
            {
                for(int i=0;i<EPOCH__HISTORY_INDEX;i++)
                {
                    if (EPOCH__RECORDS[i] == null) yield break;
                    if (EPOCH__RECORDS[i]?.Is__State__Undo ?? true) 
                        continue;
                    yield return EPOCH__RECORDS[i];
                }
            }
        }

        public Epoch(int epoch_size)
        {
            EPOCH__VALUES = new TRecord_Value[epoch_size];
            EPOCH__RECORDS = new Record[epoch_size];
        }

        public bool Append(TRecord_Value value)
        {
            if (Is__Epoch_Finished) return false;
            Is__In_Need_Of__Update = true;

            EPOCH__RECORDS[EPOCH__HISTORY_INDEX] = new Record();
            EPOCH__VALUES [EPOCH__HISTORY_INDEX++] = value;
            if (!Is__Epoch_Finished && (EPOCH__RECORDS[EPOCH__HISTORY_INDEX]?.Is__State__Undo ?? false))
            {
                EPOCH__RECORDS[EPOCH__HISTORY_INDEX].Is__State__Dead_Undo = true;
            }

            return true;
        }

        public bool Undo()
        {
            if (Is__Epoch_Empty) return false;
            Is__In_Need_Of__Update = true;

            EPOCH__RECORDS[--EPOCH__HISTORY_INDEX].Is__State__Undo = true;

            return true;
        }

        public bool Redo()
        {
            if (Is__Epoch_Finished) return false;

            if (EPOCH__RECORDS[EPOCH__HISTORY_INDEX] == null) return false;
            if (EPOCH__RECORDS[EPOCH__HISTORY_INDEX].Is__State__Dead_Undo) return false;
            Is__In_Need_Of__Update = true;

            EPOCH__RECORDS[EPOCH__HISTORY_INDEX++].Is__State__Undo = false;

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
    protected TRecord_Value Preparing__Value { get; set; }
    public bool Is__Preparing__Value { get; private set; }

    public readonly int EPOCH__SIZE;
    public readonly int EPOCH__COUNT;
    private int GRID__WIDTH;
    private int GRID__HEIGHT;

    protected readonly Epoch[] EPOCHS;
    internal int EPOCH__INDEX = 0;
    private int EPOCH__INDEX__OLDEST = 0;
    private int EPOCH__COUNT_GENERATED = 1;

    public int Quantity__Of__Records
        => ((EPOCH__COUNT_GENERATED - 1) * EPOCH__SIZE) + Epoch__Currently_Indexed.EPOCH__HISTORY_INDEX;

    public int Quantity__Of__Epochs_Generated
        => EPOCH__COUNT_GENERATED;

    public int Index__Current
        => EPOCH__INDEX;
    public int Index__Next_Epoch
        => 
        (EPOCH__INDEX + 1) 
        % 
        EPOCHS.Length
        ;
    public int Index__Previous_Epoch
        => 
        (
            (EPOCH__INDEX - 1)
            %
            EPOCHS.Length
            +
            EPOCHS.Length
        )
        %
        EPOCHS.Length
        ;

    public int Get__Index_From__Oldest_Epoch(int i)
        => (i + EPOCH__INDEX__OLDEST) % EPOCHS.Length;
    public int Get__Index_Offset_From__Oldest_Epoch(int i, int step)
        => (((i + step) % EPOCHS.Length) * EPOCHS.Length) % EPOCHS.Length;

    protected internal Epoch Epoch__Currently_Indexed
        => EPOCHS[EPOCH__INDEX];
    private bool Is__Previous_Epoch_Existant
        => EPOCHS[Index__Previous_Epoch] != null;

    protected internal Epoch Epoch__Active
    {
        get 
        {
            if (Epoch__Currently_Indexed.Is__Epoch_Undone && Is__Previous_Epoch_Existant)
            {
                Private_Move__Epoch_Index(is__progressing_forwards_or_backwards: false);
                return Epoch__Currently_Indexed;
            }
            if (Epoch__Currently_Indexed.Is__Epoch_Finished)
            {
                Private_Move__Epoch_Index
                (
                    is__progressing_forwards_or_backwards: true, 
                    is__creating_new_epoch: true
                );
            }

            return Epoch__Currently_Indexed;
        }
    }

    //TODO: check all epochs, not just current.
    //However this works for what we will currently
    //support.
    public bool Is__In_Need_Of__Update
        => Epoch__Currently_Indexed.Is__In_Need_Of__Update;
    public bool Is__Current_Epoch_Finished
        => Epoch__Currently_Indexed.Is__Epoch_Finished;

    public History
    (
        int history_epoch_size, 
        int history_epoch_count
    )
    {
        EPOCH__SIZE = history_epoch_size;
        EPOCH__COUNT = history_epoch_count;
        EPOCHS =
            new Epoch[history_epoch_count];
        EPOCHS[0] = new Epoch(EPOCH__SIZE);
    }

    /// <summary>
    /// Records the given value as a preparing value.
    /// Once Finish() is invoked, the preparing value will be
    /// moved onto the latest history epoch.
    /// </summary>
    public virtual void Prepare(TRecord_Value hot_value)
    {
        Preparing__Value = hot_value;
        Is__Preparing__Value = true;
    }

    public virtual void Unprepare()
        => Is__Preparing__Value = false;

    /// <summary>
    /// Moves the preparing value from Prepare(hot_value)
    /// to the latest history epoch. If there is no value being
    /// prepared, nothing happens.
    /// </summary>
    public virtual void Finish()
    {
        if (!Is__Preparing__Value) return;
        Is__Preparing__Value = false;

        Epoch__Active.Append(Preparing__Value);
    }

    /// <summary>
    /// Skip preperation and add the value straight
    /// into history.
    /// </summary>
    public virtual void Append(TRecord_Value value)
    {
        Epoch__Active.Append(value);
    }

    public virtual void Undo()
    {
        Epoch__Active?.Undo();
    }

    public virtual void Redo()
    {
        Epoch__Active.Redo();
    }

    public virtual void Clear()
    {
        EPOCH__COUNT_GENERATED = 1;
        EPOCH__INDEX = 0;
        EPOCHS[0] = new Epoch(EPOCH__SIZE);
        for(int i=1;i<EPOCH__COUNT;i++)
            EPOCHS[i] = null!;
    }

    protected IEnumerable<Epoch> Get__Epochs
    (
        bool is__only_getting__updated_epochs = true,
        bool is__updating__epochs = false
    )
    {
        for(int i=0;i<EPOCHS.Length;i++)
        {
            int index = (i + EPOCH__INDEX__OLDEST) % EPOCHS.Length;

            if (EPOCHS[index] == null) yield break;

            if (is__only_getting__updated_epochs && !EPOCHS[index].Is__In_Need_Of__Update)
                continue;
            if (is__updating__epochs)
                EPOCHS[index].Is__In_Need_Of__Update = false;

            yield return EPOCHS[index];
        }
    }

    /// <summary>
    /// Compiles all epochs down to a single
    /// aggregate value. The resulting value
    /// as a result of the history of records.
    /// </summary>
    public abstract TAggregation Aggregate__Epochs(ref bool error);

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

        if (is__creating_new_epoch || (is__progressing_forwards_or_backwards && EPOCHS[EPOCH__INDEX] == null))
        {
            if (is__progressing_forwards_or_backwards && EPOCHS[EPOCH__INDEX] != null)
                EPOCH__INDEX__OLDEST = Index__Next_Epoch;

            EPOCHS[EPOCH__INDEX] =
                new Epoch(EPOCH__SIZE);

            Handle_New__Epoch();
            EPOCH__COUNT_GENERATED = (EPOCH__COUNT_GENERATED < EPOCH__COUNT) ? EPOCH__COUNT_GENERATED + 1 : EPOCH__COUNT;
        }
    }

    protected virtual void Handle_New__Epoch() { }
}
