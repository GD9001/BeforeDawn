using UnityEngine;
using System.Collections;
//���ű��Ǹ���Unity���ṩ�ĵ�һ�˳ƿ������е�CharacterMotor��js�ű��ı�ģ������һЩԭ���ű�����û�еĹ���
//����custom code���ֵĴ����Ƕ�����ӵĴ���

[RequireComponent(typeof(CharacterController))]//�˽ű������ص���Ϸ���������н�ɫ���������
public class PlayerController : MonoBehaviour {
	public bool canControl = true;//��ɫ�Ƿ��ڿɿ�״̬
	public bool useFixedUpdate = true;//�Ƿ�ʹ��FixedUpdate����������漰��������㽨��ʹ��FixedUpdate
	
	#region custom code
	[HideInInspector]//�ڼ�����ͼ�����ر�����������ʾ��������
	public bool running;//�����ܶ�״̬��־λ
	[HideInInspector]
	public bool walking;//�����߶�״̬��־λ
	[HideInInspector]
	public bool canRun;//�Ƿ��ܹ��ܶ���־λ
	
	private GameObject mainCamera = null;//���������������
	
	[HideInInspector]
	public bool onLadder = false;//�Ƿ���������
	private float ladderHopSpeed = 6.0f;//�����ٶ�
	#endregion
	
	[System.NonSerialized]//����Unity��Ҫ���л��ñ��������ڼ�����ͼ�в���ʾ�ñ���
	public Vector3 inputMoveDirection = Vector3.zero;//��ɫ�ƶ��������
	
	[System.NonSerialized]
	public bool inputJump = false;//�Ƿ�������־λ
	
	#region custom code
	[System.NonSerialized]
	public bool inputRun = false;//�Ƿ�����ܶ���־λ
	
	[System.NonSerialized]
	public bool inputCrouch = false;//�Ƿ��¶ױ�־λ
	#endregion
	
	[System.Serializable]//�������л��󣬸������͵ı��������Inspector��ͼ����ʾ��
	public class PlayerControllerMovement {
		[HideInInspector]//��Inspector��ͼ�����ر���
		public float maxForwardSpeed = 10.0f;//�����ǰ�ٶ�
		[HideInInspector]
		public float maxSidewaysSpeed = 10.0f;//�������ٶ�
		[HideInInspector]
		public float maxBackwardsSpeed = 10.0f;//�������ٶ�
		
		#region custom code
		public float walkSpeed = 6.0f;//�����ٶ�
		public float runSpeed = 9.0f;//�ܶ��ٶ�
		
		public bool canCrouch = true;//�Ƿ�����¶ױ�־λ
		public float crouchSpeed = 3.0f;//�¶��ٶ�
		public float crouchHeight = 1.2f;//�¶�ʱ�ĸ߶�
		public float crouchSmooth = 0.1f;//�¶�ʱ��ƽ��
		#endregion
		
		//�����������ߣ��������ƽ�ɫ��б���ϵ��ٶ�
		public AnimationCurve slopeSpeedMultiplier = new AnimationCurve(new Keyframe(-90, 1), new Keyframe(0, 1), new Keyframe(90, 0));
		
		public float maxGroundAcceleration = 30.0f;//��������ٶ�
		public float maxAirAcceleration = 20.0f;//�����м��ٶ�
		
		public float gravity = 10.0f;//�������ٶ�
		public float maxFallSpeed = 20.0f;//��������ٶ�
	
		[HideInInspector]
		public bool enableGravity = true;//ʹ���������ٶȱ�־λ

		[System.NonSerialized]
		public CollisionFlags collisionFlags;//������ײ��־λ

		[System.NonSerialized]
		public Vector3 velocity;//׷�ٽ�ɫ�ĵ�ǰ�ٶ�

		[System.NonSerialized]
		public Vector3 frameVelocity = Vector3.zero;//׷��û�����ʱ�Ľ�ɫ�ٶ�
	
		[System.NonSerialized]
		public Vector3 hitPoint = Vector3.zero;//������ײ��
	
		[System.NonSerialized]
		public Vector3 lastHitPoint = new Vector3(Mathf.Infinity, 0, 0);//��һ����ײ��
	}
	
	public PlayerControllerMovement movement = new PlayerControllerMovement();//����һ��ʵ��
	
	#region custom code
	[HideInInspector]
	public bool crouch;//�¶ױ�־λ
	private float standardHeight;//��׼�߶�
	private GameObject lookObj;//��������壬��֤�ӽǵ���ȷ
	private float centerY;//��ֱ�������ĵ�
	private bool canStand;//�ܷ�վ���жϱ���
	private bool canStandCrouch = true;//�ܷ�վ���¶��жϱ���
	#endregion

	public enum PlayerMovementTransferOnJump {
		None, //���������ڵ����ϵ��ٶȵ�Ӱ��
		InitTransfer, //��������ٶ�������ϵ��ٶ���ͬ��Ȼ���𽥼�С��ֹͣ
		PermaTransfer, //��������ٶ�������ϵ��ٶ���ͬ�����ұ����ٶ�ֱ�����
		PermaLocked //�������һ���˶�����ص����𣬽����ŵ���һ���ƶ�
	}
	
	[System.Serializable]
	public class PlayerControllerJumping {
		public bool enabled = true;//�Ƿ�����������
	
		public float baseHeight = 1.0f;//������������������������ʱ�Ļ����߶�
		
		public float extraHeight = 4.1f;//������ʱ��ʱ�䰴ס������ʱ����Ķ���߶�
		
		public float perpAmount = 0.0f;//��ɫ����������������վ����ĸ߶�
		
		public float steepPerpAmount = 0.5f;//��ɫ����������������վ���ͱ���ĸ߶�
		
		[System.NonSerialized]
		public bool jumping = false;//�Ƿ�������־λ�����ڿ���ʱ�Ƿ���Ȼ����
		
		[System.NonSerialized]
		public bool holdingJumpButton = false;//�Ƿ�ס��������־λ
	
		[System.NonSerialized]
		public float lastStartTime = 0.0f;//�ϴο�ʼ��ʱ�䣬���ڼ�ʱ��ס��������ʱ��
		
		[System.NonSerialized]
		public float lastButtonDownTime = -100;//�ϴΰ��°�ť��ʱ��
		
		[System.NonSerialized]
		public Vector3 jumpDir = Vector3.up;//������
	}
	
	public PlayerControllerJumping jumping = new PlayerControllerJumping();//�����������ʵ��
	
	[System.Serializable]
	public class PlayerControllerMovingPlatform {
		public bool enabled = true;//�Ƿ������ƶ�����
		
		public PlayerMovementTransferOnJump movementTransfer = PlayerMovementTransferOnJump.PermaTransfer;//�ƶ�ģʽ
		
		[System.NonSerialized]
		public Transform hitPlatform;//��ײƽ̨
		
		[System.NonSerialized]
		public Transform activePlatform;//�ƽ̨
		
		[System.NonSerialized]
		public Vector3 activeLocalPoint;//����ı��ص�
		
		[System.NonSerialized]
		public Vector3 activeGlobalPoint;//�����ȫ�ֵ�
		
		[System.NonSerialized]
		public Quaternion activeLocalRotation;//����ı�����ת
		
		[System.NonSerialized]
		public Quaternion activeGlobalRotation;//�����ȫ����ת
		
		[System.NonSerialized]
		public Matrix4x4 lastMatrix;//��һ����
		
		[System.NonSerialized]
		public Vector3 platformVelocity;//ƽ̨�ٶ�
		
		[System.NonSerialized]
		public bool newPlatform;//�Ƿ�Ϊ�µ�ƽ̨��־λ
	}

	public PlayerControllerMovingPlatform movingPlatform = new PlayerControllerMovingPlatform();//ʵ��������
	
	[System.Serializable]
	public class PlayerControllerSliding {
		public bool enabled = true;//�Ƿ��ɫ��һ�����͵ı��滬��
		
		public float slidingSpeed = 15;//��б���ϵĻ����ٶ�
		
		public float sidewaysControl = 1.0f;//����ܹ����ƵĻ�������Ķ��٣���Ϊ0.5ʱ��ʾ��ҿ��԰��ٲ໬
		
		public float speedControl = 0.4f;//�����ٶȣ�����ʱ�ٶ����ӣ�����ʱ�ٶȼ�С
	}

	public PlayerControllerSliding sliding = new PlayerControllerSliding();//ʵ��������
	
	#region custom code
	[System.Serializable]
	public class PlayerControllerPushing {	
		public bool canPush = true;//�Ƿ���ƶ������־λ
		public float pushPower = 2.0f;//�ƶ�������
	}
	public PlayerControllerPushing pushing;//��������
	#endregion
		
	[System.NonSerialized]
	public bool grounded = true;//��ɫ�Ƿ���ر�־λ
	
	[System.NonSerialized]
	public Vector3 groundNormal = Vector3.zero;//���淨����
	
	private Vector3 lastGroundNormal = Vector3.zero;//�ϴ���ط�����

	private Transform tr;//��������
	
	private CharacterController controller;//������ɫ����������
	
	void Awake () {
		controller = GetComponent<CharacterController>();//�õ���ɫ���������
		tr = transform;//��ֵ
		#region custom code
		standardHeight = controller.height;//�õ���ɫ��׼���
		lookObj = GameObject.FindWithTag("LookObject");//�õ���Ϸ����
		centerY = controller.center.y;//�õ���ɫ��ֱ�����ϵ����ĵ�
		mainCamera = GameObject.FindWithTag("MainCamera");//�õ������������
		canRun = true;//��ֵ
		canStand = true;//��ֵ
		#endregion
	}
	
	private void UpdateFunction() {
		Vector3 velocity = movement.velocity;//��ʵ�ʵ��ٶȸ�ֵ��һ����ʱ���ٶȱ���

		velocity = ApplyInputVelocityChange(velocity);//������ҵ�����������±���ֵ
	
		if(movement.enableGravity){//����������ʱ
			if(crouch && inputJump)//�¶׹����У���Ұ���������������
				return;
			velocity = ApplyGravityAndJumping (velocity);//Ӧ�������������������Ǹ�һ��Y�������ϵĳ��ٶȣ�ͬʱ��һ���������ٶȣ�ʵ�����ף������ٶ�
		}
		
		Vector3 moveDistance = Vector3.zero;//�ƶ�λ��
		if (MoveWithPlatform()) {//����ƽ̨���ƶ�ʱ
			Vector3 newGlobalPoint = movingPlatform.activePlatform.TransformPoint(movingPlatform.activeLocalPoint);//�õ��µ�ȫ�ֵ�
			moveDistance = (newGlobalPoint - movingPlatform.activeGlobalPoint);//�����ƶ�λ��
			if (moveDistance != Vector3.zero)//����ƶ�λ�Ʋ�Ϊ��ʱ
				controller.Move(moveDistance);//�ƶ���ɫ
			
	        Quaternion newGlobalRotation = movingPlatform.activePlatform.rotation * movingPlatform.activeLocalRotation;//�õ��µ�ȫ����ת
	        Quaternion rotationDiff = newGlobalRotation * Quaternion.Inverse(movingPlatform.activeGlobalRotation);//������ת��Ԫ��
	        
	        float yRotation = rotationDiff.eulerAngles.y;//�õ���ɫ��Y�᷽���ϵ���ת�Ƕ�
	        if (yRotation != 0) {//��Y�����ϵ���ת�ǶȲ�Ϊ��ʱ
		        tr.Rotate(0, yRotation, 0);//ת����ɫ
	        }
		}

		Vector3 lastPosition = tr.position;//������һ��λ�������ٶȼ���

		Vector3 currentMovementOffset = velocity * Time.deltaTime;//���㵱ǰ�ƶ�ƫ����
	
		float pushDownOffset = Mathf.Max(controller.stepOffset, new Vector3(currentMovementOffset.x, 0, currentMovementOffset.z).magnitude);//��ýϴ�ֵ
		if (grounded)//������
			currentMovementOffset -= pushDownOffset * Vector3.up;//���㵱ǰ�ƶ�ƫ����
	
		movingPlatform.hitPlatform = null;//���ñ���
		groundNormal = Vector3.zero;//���ñ���
	
		movement.collisionFlags = controller.Move (currentMovementOffset);//�ƶ���ɫ
	
		movement.lastHitPoint = movement.hitPoint;//��ֵ
		lastGroundNormal = groundNormal;//��ֵ
	
		if (movingPlatform.enabled && movingPlatform.activePlatform != movingPlatform.hitPlatform) {//���ƶ�ƽ̨�����һƽ̨���ǽӴ�����ƽ̨ʱ
			if (movingPlatform.hitPlatform != null) {//����Ӵ�����ƽ̨����ʱ
				movingPlatform.activePlatform = movingPlatform.hitPlatform;//���ƽ̨����Ϊ�Ӵ�����ƽ̨
				movingPlatform.lastMatrix = movingPlatform.hitPlatform.localToWorldMatrix;//���Ӵ�����ƽ̨�ı�������ϵ����������ϵ�ľ���ֵ��lastMatrix����
				movingPlatform.newPlatform = true;//���ñ�־λΪ��
			}
		}
		
		Vector3 oldHVelocity = new Vector3(velocity.x, 0, velocity.z);//��¼��һ��ˮƽ�����ٶ�
		movement.velocity = (tr.position - lastPosition) / Time.deltaTime;//�����ƶ��ٶ�
		Vector3 newHVelocity = new Vector3(movement.velocity.x, 0, movement.velocity.z);//��¼�µ�ˮƽ�����ٶ�
		
		if (oldHVelocity == Vector3.zero) {//����һ�μ�¼���ٶ�Ϊ��ʱ
			movement.velocity = new Vector3(0, movement.velocity.y, 0);//���¸�ֵ
		}
		else {
			float projectedNewVelocity = Vector3.Dot(newHVelocity, oldHVelocity) / oldHVelocity.sqrMagnitude;//�������ٶ�ˮƽ�ٶ�����һ��ˮƽ�ٶȷ����ϵ�ͶӰ
			movement.velocity = oldHVelocity * Mathf.Clamp01(projectedNewVelocity) + movement.velocity.y * Vector3.up;//���¼����ٶ�
		}
	
		if (movement.velocity.y < velocity.y - 0.001f) {//����ֱ�����ٶ�С��ĳ��ֵʱ
			if (movement.velocity.y < 0) {//�����ֱ�����ٶ�С����ʱ
				movement.velocity.y = velocity.y;//��ֵ
			}
			else {
				jumping.holdingJumpButton = false;//����������ť
			}
		}
	
		if (grounded && !IsGroundedTest()) {//����ɫ��ص��Ƕ�ʧ�����ʱ�����������������
			grounded = false;//�������Ϊ��
		
			if (movingPlatform.enabled &&
				(movingPlatform.movementTransfer == PlayerMovementTransferOnJump.InitTransfer ||
				movingPlatform.movementTransfer == PlayerMovementTransferOnJump.PermaTransfer)
			) {//����������ʱ
				movement.frameVelocity = movingPlatform.platformVelocity;//��ֵ
				movement.velocity += movingPlatform.platformVelocity;//�����ƶ��ٶ�
			}
			
			SendMessage("OnFall", SendMessageOptions.DontRequireReceiver);//����OnFall����
			tr.position += pushDownOffset * Vector3.up;//�����ɫλ��
		}
		else if (!grounded && IsGroundedTest()) {//����ɫû����أ�����վ��ĳ����������
			grounded = true;//���ñ�־λΪ���
			jumping.jumping = false;//����������־λΪ�٣���վ��ĳ�������ϲ�������
			SubtractNewPlatformVelocity();//���÷���
			
			SendMessage("OnLand", SendMessageOptions.DontRequireReceiver);//��OnLand����������Ϣ
		}
	
		if (MoveWithPlatform()) {//���ƶ���ƽ̨�� 
			movingPlatform.activeGlobalPoint = tr.position + Vector3.up * (controller.center.y - controller.height*0.5f + controller.radius);//�����ƶ�ƽ̨�Ļȫ�ֵ�
			movingPlatform.activeLocalPoint = movingPlatform.activePlatform.InverseTransformPoint(movingPlatform.activeGlobalPoint);//�����ƶ�ƽ̨�Ļ�����
			
	        movingPlatform.activeGlobalRotation = tr.rotation;//��ֵ
	        movingPlatform.activeLocalRotation = Quaternion.Inverse(movingPlatform.activePlatform.rotation) * movingPlatform.activeGlobalRotation; //�����ƶ�ƽ̨���������̬
		}
	}
	
	void FixedUpdate() {
		if (movingPlatform.enabled) {//�������ƶ�ƽ̨ʱ
			if (movingPlatform.activePlatform != null) {//���ƽ̨����ʱ
				if (!movingPlatform.newPlatform) {//���ƶ�ƽ̨������ƽ̨ʱ
					Vector3 lastVelocity = movingPlatform.platformVelocity;//��ֵ
					
					movingPlatform.platformVelocity = (
						movingPlatform.activePlatform.localToWorldMatrix.MultiplyPoint3x4(movingPlatform.activeLocalPoint)
						- movingPlatform.lastMatrix.MultiplyPoint3x4(movingPlatform.activeLocalPoint)
					) / Time.deltaTime;//�����ƶ�ƽ̨�ϵ��ٶ�
				}
				movingPlatform.lastMatrix = movingPlatform.activePlatform.localToWorldMatrix;//���ƽ̨�ı�������ϵ����������ϵ�ľ���ֵ��lastMatrix����
				movingPlatform.newPlatform = false;//���ñ�־λΪ��
			}
			else {
				movingPlatform.platformVelocity = Vector3.zero;	//��ֵ
			}
		}
		
		if(useFixedUpdate) {//������FixedUpdate����ʱ
			UpdateFunction();//���÷���
		}
	}
	
	void Update () {
		if(!useFixedUpdate) {//��û������FixedUpdate����ʱ
			UpdateFunction();//���÷���
		}
		
		#region custom code
		if(Input.GetAxis("Vertical") > 0.1f && inputRun && canRun && walking){//��������Щ����ʱ
			if(canStand && canStandCrouch){//����վ���������¶�ʱ
				OnRunning();//�����ܶ�����
			}
		}else{
			OffRunning();//���ý�ֹ�ܶ�����
		}	
			
		if ((movement.velocity.x > 0.01f || movement.velocity.z > 0.01f) || (movement.velocity.x < -0.01f || movement.velocity.z < -0.01f)) {//��������Щ����ʱ
			walking = true;//�߶���־λΪ��
		}else{
			walking = false;//�߶���־λ��Ϊ��
		}
		
		if(!canControl)//�����ɫ���ɿ�,��ִ�к���Ĵ���
			return;
		
		if(movement.canCrouch){//�������¶�ʱ
			if(!onLadder){//����ɫû������ʱ
				Crouch();//�����¶׷���
			}
		}
		
		if(onLadder){//��������ʱ
			grounded = false;//������ر�־λ��
			crouch = false;//�����¶ױ�־Ϊ��
		}
		
		if(!crouch && controller.height < standardHeight-0.01f){//��û���¶��ҽ�ɫ�������߶�С�ڽ�ɫ��׼�߶�0.01ʱ
			controller.height = Mathf.Lerp(controller.height, standardHeight, Time.deltaTime/movement.crouchSmooth);//�Խ�ɫ�������ĸ߶����Բ�ֵ��ʹ��ɫ������ƽ������׼�߶�
			
			Vector3 tempCenter = controller.center;//����ֱ����controller.center��y = 0.5f;���������Ǵ���ģ�����ʹ����ʱ��������controller.center�����͵����Խ��и�ֵ
			tempCenter.y = Mathf.Lerp(tempCenter.y, centerY, Time.deltaTime/movement.crouchSmooth);//�Խ�ɫ�����������ĵ�Y�������Բ�ֵ
			controller.center = tempCenter;//��ֵ
			
			Vector3 tempPos = lookObj.transform.localPosition;//ͬ��
			tempPos.y = Mathf.Lerp(tempPos.y, standardHeight, Time.deltaTime/movement.crouchSmooth);//��������������Բ�ֵ��ʹ��������Ž�ɫ�߶ȵı任�����ƶ�
			lookObj.transform.localPosition = tempPos;//��ֵ
		}
		#endregion
	}
	
	#region custom code
	void Crouch(){
		Vector3 up = transform.TransformDirection(Vector3.up);//�õ���ɫ��Y��������
	   	RaycastHit hit;//������ײ����
		CharacterController charCtrl = GetComponent<CharacterController>();//�õ���ɫ���������
	    Vector3 p1 = transform.position;//�����ɫλ��
		if(inputCrouch && !running && canStand){//������C���ҽ�ɫ�������ܶ�״̬��ͬʱ����վ��ʱ����ֻ��վ��״̬ʱ����C����������
			crouch = !crouch;//����־λ�÷�
		}

	   	 if (!Physics.SphereCast (p1, charCtrl.radius, transform.up, out hit, standardHeight)) {//������ײ���
			if(inputJump && crouch){//������ʱ�������¶�
				crouch = false;//���ñ�־λ
			}
			if(running && crouch){//���ܶ�ʱ�������¶�
				crouch = false;//���ñ�־λ
			}
			if(crouch){//���¶ױ�־λΪ��ʱ�¶�
				canStand = true;//���ñ�־λ
			}
			canStandCrouch = true;//���ñ�־λ
	   	}else{
	   		if(crouch){//���¶ױ�־λΪ��ʱ
	   			canStand = false;//���ñ�־λ
	   		}
	   		canStandCrouch = false;//���ñ�־λ
	   	}
		
		if(crouch){//����¶ױ�־λΪ��
			if(controller.height < movement.crouchHeight+0.01f && controller.height > movement.crouchHeight-0.01f)//�����Щ��������
				return;//����
			controller.height = Mathf.Lerp(controller.height, movement.crouchHeight, Time.deltaTime/movement.crouchSmooth);//ʹ�����Բ�ֵ�ı��ɫ�������߶�
			
			Vector3 tempCenterY = controller.center;//�õ���ɫ���������ĵ�
			tempCenterY.y = Mathf.Lerp(tempCenterY.y, movement.crouchHeight/2, Time.deltaTime/movement.crouchSmooth);//���Բ�ֵ�ı��ɫ���������ĵ�λ��
			controller.center = tempCenterY;//��ֵ
				
			Vector3 tempPos = lookObj.transform.localPosition;//�õ�lookObj�����λ��
			tempPos.y = Mathf.Lerp(tempPos.y, movement.crouchHeight, Time.deltaTime/movement.crouchSmooth);//���Բ�ֵ�ı�λ��
			lookObj.transform.localPosition = tempPos;//��ֵ
				
			movement.maxForwardSpeed = movement.crouchSpeed;//��ֵ
			movement.maxSidewaysSpeed = movement.crouchSpeed;//��ֵ
			movement.maxBackwardsSpeed = movement.crouchSpeed;//��ֵ
		}
	}
	
	void OnRunning (){
		running = true;//�����ܶ���־λΪ��
		movement.maxForwardSpeed = movement.runSpeed;//�����ǰ���ٶȸ�ֵ
		movement.maxSidewaysSpeed = movement.runSpeed;//����������ٶȸ�ֵ
		jumping.extraHeight = jumping.baseHeight + 0.15f;//��ֵ
	}
	
	void OffRunning (){
		running = false;//�����ܶ���־λΪ��
		if(crouch) { return; }//����¶�ʱ��ִ�к�������
			
		movement.maxForwardSpeed = movement.walkSpeed;//�����ǰ���ٶȸ�ֵ
		movement.maxSidewaysSpeed = movement.walkSpeed;//����������ٶȸ�ֵ
		movement.maxBackwardsSpeed = movement.walkSpeed/2;//����������ٶȸ�ֵ
		jumping.extraHeight = jumping.baseHeight;//��ֵ������ɫ�߶�ʱ������Ķ���߶�Ϊ�����Ĭ�ϸ߶�
	}
	#endregion
	
	private Vector3 ApplyInputVelocityChange(Vector3 velocity) {
		if(!canControl) {//������ɿ�
			inputMoveDirection = Vector3.zero;//ǿ���ƶ����������Ϊ������
		}
		
		Vector3 desiredVelocity;//���������ٶȱ���
		if(grounded && TooSteep()) {//����ɫ������ڶ�����
			//ʵ���������ٶ�
			desiredVelocity = new Vector3(groundNormal.x, 0, groundNormal.z).normalized;
			//�����ƶ������ڻ��з����ϵ�ͶӰ
			Vector3 projectedMoveDir = Vector3.Project(inputMoveDirection, desiredVelocity);
			desiredVelocity = desiredVelocity + projectedMoveDir * sliding.speedControl + (inputMoveDirection - projectedMoveDir) * sliding.sidewaysControl;//���������ٶ�
			desiredVelocity *= sliding.slidingSpeed;//���������ٶ�
		}
		else {
			desiredVelocity = GetDesiredHorizontalVelocity();//�õ�ˮƽ�����ϵ��ٶ�
		}

		if (movingPlatform.enabled && movingPlatform.movementTransfer == PlayerMovementTransferOnJump.PermaTransfer) {//��������ʱ
			desiredVelocity += movement.frameVelocity;//���������ٶ�
			desiredVelocity.y = 0;//��ֵ
		}
		
		if (grounded) {//������
			desiredVelocity = AdjustGroundVelocityToNormal(desiredVelocity, groundNormal);//���÷������������ٶ�
		}
		else {
			velocity.y = 0;//��ֵ
		}
		
		float maxVelocityChange = GetMaxAcceleration(grounded) * Time.deltaTime;//�������ı��ٶ�
		Vector3 velocityChangeVector = (desiredVelocity - velocity);//�����ٶȸı�����
		if (velocityChangeVector.sqrMagnitude > maxVelocityChange * maxVelocityChange) {//��������ʱ
			velocityChangeVector = velocityChangeVector.normalized * maxVelocityChange;//�����ٶȸı�����
		}
		
		if (grounded || canControl)//����ػ��߿ɿ�
			velocity += velocityChangeVector;//�ı��ٶ�
		
		if (grounded) {//�����
			velocity.y = Mathf.Min(velocity.y, 0);//�����ٶ�y����
		}
		return velocity;//�����ٶ�
	}
	
	private Vector3 ApplyGravityAndJumping (Vector3 velocity) {
	
		if (!inputJump || !canControl) {//��û��������߲��ɿ�
			jumping.holdingJumpButton = false;//���ñ�־λ
			jumping.lastButtonDownTime = -100;//��ֵ
		}
		
		if (inputJump && jumping.lastButtonDownTime < 0 && canControl)//����������ʱ
			jumping.lastButtonDownTime = Time.time;//��ȡ��Ϸʱ�䲢��ֵ����һ�ε��°�ťʱ��
		
		if (grounded)//������
			velocity.y = Mathf.Min(0, velocity.y) - movement.gravity * Time.deltaTime;//�����ٶ�y����
		else {
			velocity.y = movement.velocity.y - movement.gravity * Time.deltaTime;//�����ٶ�y����
			
			if (jumping.jumping && jumping.holdingJumpButton) {//��������Ұ�ס������ť
				if (Time.time < jumping.lastStartTime + jumping.extraHeight / CalculateJumpVerticalSpeed(jumping.baseHeight)) {//�����������
					velocity += jumping.jumpDir * movement.gravity * Time.deltaTime;//�����ٶ�
				}
			}
			
			velocity.y = Mathf.Max (velocity.y, -movement.maxFallSpeed);//�����ٶ�y����
		}
			
		if (grounded) {//������
			if (jumping.enabled && canControl && (Time.time - jumping.lastButtonDownTime < 0.2)) {//�����������
				grounded = false;//���ñ�־λ
				jumping.jumping = true;//���ñ�־λ
				jumping.lastStartTime = Time.time;//��ֵ
				jumping.lastButtonDownTime = -100;//��ֵ
				jumping.holdingJumpButton = true;//���ñ�־λ
				
				if (TooSteep()) {//���÷����ж��Ƿ�̫����
					jumping.jumpDir = Vector3.Slerp(Vector3.up, groundNormal, jumping.steepPerpAmount);//����������
				}
				else {
					jumping.jumpDir = Vector3.Slerp(Vector3.up, groundNormal, jumping.perpAmount);//����������
				}
				
				velocity.y = 0;//��ֵ
				velocity += jumping.jumpDir * CalculateJumpVerticalSpeed (jumping.baseHeight);//�����ٶ�
				
				if (movingPlatform.enabled &&
					(movingPlatform.movementTransfer == PlayerMovementTransferOnJump.InitTransfer ||
					movingPlatform.movementTransfer == PlayerMovementTransferOnJump.PermaTransfer)
				) {//�����������
					movement.frameVelocity = movingPlatform.platformVelocity;//����ÿ֡�ٶ�
					velocity += movingPlatform.platformVelocity;//�����ٶ�
				}
				
				SendMessage("OnJump", SendMessageOptions.DontRequireReceiver);//������Ϣ������OnJump����
			}
			else {
				jumping.holdingJumpButton = false;//���ñ�־λ
			}
		}
		
		return velocity;//�����ٶ�
	}
	
	void OnControllerColliderHit (ControllerColliderHit hit) {
		if (hit.normal.y > 0 && hit.normal.y > groundNormal.y && hit.moveDirection.y < 0) {//�����������
			if ((hit.point - movement.lastHitPoint).sqrMagnitude > 0.001f || lastGroundNormal == Vector3.zero)//�����������
				groundNormal = hit.normal;//��ֵ
			else
				groundNormal = lastGroundNormal;//��ֵ
			
			movingPlatform.hitPlatform = hit.collider.transform;//��ֵ
			movement.hitPoint = hit.point;//��ֵ
			movement.frameVelocity = Vector3.zero;//��ֵ
		}
		
		#region custom code
		if(pushing.canPush){//���������
			Rigidbody body = hit.collider.attachedRigidbody;//�õ����߼�⵽�Ķ����ϸ��ӵĸ���
			if (body == null || body.isKinematic)//������岻���ڻ��߸����isKinematic����Ϊ��
				return;//����
	
			if (hit.moveDirection.y < -0.3f)//������ƶ��Ķ���Ƚ�ɫ��
				return;//����
	
			Vector3 pushDir = new Vector3 (hit.moveDirection.x, 0, hit.moveDirection.z);//�����ƶ��ķ��������
		
			body.velocity = pushDir * pushing.pushPower;//�����ٶ�
		}
		#endregion
	}
	
	private IEnumerator SubtractNewPlatformVelocity () {
		if (movingPlatform.enabled &&
			(movingPlatform.movementTransfer == PlayerMovementTransferOnJump.InitTransfer ||
			movingPlatform.movementTransfer == PlayerMovementTransferOnJump.PermaTransfer)
		) {//�����������
			if (movingPlatform.newPlatform) {//�������ƽ̨
				Transform platform = movingPlatform.activePlatform;//��ֵ
				yield return new WaitForFixedUpdate();//�ȴ�FixedUpdateִ��һ��
				yield return new WaitForFixedUpdate();//�ȴ�FixedUpdateִ��һ��
				if (grounded && platform == movingPlatform.activePlatform)
					yield return 1;//�ȴ�һ֡
			}
			movement.velocity -= movingPlatform.platformVelocity;//�����ٶ�
		}
	}
	
	private bool MoveWithPlatform () {
		return (
			movingPlatform.enabled
			&& (grounded || movingPlatform.movementTransfer == PlayerMovementTransferOnJump.PermaLocked)
			&& movingPlatform.activePlatform != null
		);//�����Ƿ���ƽ̨�ƶ��Ĳ���ֵ
	}
	
	private Vector3 GetDesiredHorizontalVelocity () {
		Vector3 desiredLocalDirection = tr.InverseTransformDirection(inputMoveDirection);//�������뱾���ٶ�
		float maxSpeed = MaxSpeedInDirection(desiredLocalDirection);//���÷��������ƶ������ϵ�����ٶ�
		if (grounded) {//������
			// Modify max speed on slopes based on slope speed multiplier curve
			float movementSlopeAngle = Mathf.Asin(movement.velocity.normalized.y)  * Mathf.Rad2Deg;//�����¶�
			maxSpeed *= movement.slopeSpeedMultiplier.Evaluate(movementSlopeAngle);//��������ٶ�
		}
		return tr.TransformDirection(desiredLocalDirection * maxSpeed);//���ؽ��
	}
	
	private Vector3 AdjustGroundVelocityToNormal (Vector3 hVelocity, Vector3 groundNormal) {
		Vector3 sideways = Vector3.Cross(Vector3.up, hVelocity);//����
		return Vector3.Cross(sideways, groundNormal).normalized * hVelocity.magnitude;//���ؼ�����
	}
	
	private bool IsGroundedTest () {//�Ƿ���ز���
		return (groundNormal.y > 0.01f);//����
	}
	
	float GetMaxAcceleration (bool grounded) {//�õ������ٶ�
		if (grounded)//������
			return movement.maxGroundAcceleration;//���ص����ϵ������ٶ�
		else
			return movement.maxAirAcceleration;//���ؿ��е������ٶ�
	}
	
	float CalculateJumpVerticalSpeed (float targetJumpHeight) {//����������ˮƽ�ٶ�
		return Mathf.Sqrt (2 * targetJumpHeight * movement.gravity);//����������ˮƽ�ٶ�
	}
	
	public bool IsJumping () {
		return jumping.jumping;//�����Ƿ�������־λֵ
	}
	
	public bool IsSliding () {
		return (grounded && sliding.enabled && TooSteep());//�����Ƿ񻬶�����ֵ
	}
	
	public bool IsTouchingCeiling () {
		return (movement.collisionFlags & CollisionFlags.CollidedAbove) != 0;//�����Ƿ�ͷ���������岼��ֵ
	}
	
	public bool IsGrounded () {
		return grounded;//�����Ƿ���ر�־λ
	}
	
	public bool TooSteep () {
		return (groundNormal.y <= Mathf.Cos(controller.slopeLimit * Mathf.Deg2Rad));//�����Ƿ�̫���ͱ�־λ
	}
	
	public Vector3 GetDirection () {
		return inputMoveDirection;//���ؽ�ɫ�ƶ����������
	}
	
	public void SetControllable (bool controllable) {
		canControl = controllable;//���ý�ɫ�Ƿ�ɿ���
	}
	
	float MaxSpeedInDirection (Vector3 desiredMovementDirection) {
		if (desiredMovementDirection == Vector3.zero)//���ٶȷ���Ϊ������ʱ
			return 0;//����
		else {
			//�������ƶ������ϵ�����ٶ�
			float zAxisEllipseMultiplier = (desiredMovementDirection.z > 0 ? movement.maxForwardSpeed : movement.maxBackwardsSpeed) / movement.maxSidewaysSpeed;
			Vector3 temp = new Vector3(desiredMovementDirection.x, 0, desiredMovementDirection.z / zAxisEllipseMultiplier).normalized;//�õ���λ����
			float length = new Vector3(temp.x, 0, temp.z * zAxisEllipseMultiplier).magnitude * movement.maxSidewaysSpeed;//�����ٶ�
			return length;//�����ٶ�
		}
	}
	
	void SetVelocity (Vector3 velocity) {
		grounded = false;//���ñ�־λ
		movement.velocity = velocity;//�����ٶ�
		movement.frameVelocity = Vector3.zero;//�����ٶȷ���
		SendMessage("OnExternalVelocity");//������Ϣ
	}
}
